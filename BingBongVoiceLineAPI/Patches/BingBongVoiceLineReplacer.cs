using BepInEx.Configuration;
using BingBongVoiceLineAPI.Config;
using BingBongVoiceLineAPI.Helpers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BingBongVoiceLineAPI.Patches
{
    static class BingBongVoiceLineReplacer
    {
        public static void TryHandleCreatedObject(UnityEngine.Object created)
        {
            try
            {
                if (created == null)
                {
                    BingBongVoiceLineAPI.Log.LogWarning("[BingBongMouthPatch] TryHandleCreatedObject called with null created object.");
                    return;
                }

                GameObject go = null;
                if (created is GameObject g) go = g;
                else if (created is Component c) go = c.gameObject;
                if (go == null)
                {
                    BingBongVoiceLineAPI.Log.LogWarning($"[BingBongMouthPatch] Created object is not a GameObject or Component. Type={created.GetType().FullName}");
                    return;
                }

                BingBong bing = go.GetComponent<BingBong>() ?? go.GetComponentInChildren<BingBong>(includeInactive: true);
                Action_AskBingBong ask = null;
                if (bing != null)
                {
                    BingBongVoiceLineAPI.Log.LogInfo($"[BingBongMouthPatch] Found BingBong component on '{go.name}' (type {bing.GetType().FullName}).");
                    ask = bing.GetComponent<Action_AskBingBong>() ?? bing.GetComponentInChildren<Action_AskBingBong>(includeInactive: true);
                }
                else
                {
                    ask = go.GetComponent<Action_AskBingBong>() ?? go.GetComponentInChildren<Action_AskBingBong>(includeInactive: true);
                }

                if (ask != null)
                {
                    BingBongVoiceLineAPI.Log.LogInfo($"[BingBongMouthPatch] Applying custom responses to Action_AskBingBong on '{go.name}' (instance type {ask.GetType().FullName}).");
                    ApplyCustomResponses(ask);
                }
                else
                {
                    BingBongVoiceLineAPI.Log.LogDebug($"[BingBongMouthPatch] No Action_AskBingBong found on '{go.name}'.");
                }
            }
            catch (Exception ex)
            {
                BingBongVoiceLineAPI.Log.LogError($"[BingBongMouthPatch] TryHandleCreatedObject exception: {ex}");
            }
        }

        public static void ApplyCustomResponses(Action_AskBingBong latest)
        {
            if (latest == null) return;

            CustomSoundManager manager = CustomSoundManager.Instance;
            if (manager == null || !manager.IsLoaded)
            {
                BingBongVoiceLineAPI.Log.LogWarning("CustomSoundManager not ready, skipping custom responses.");
                return;
            }

            List<BingBongResponseConfigEntry> configEntries = ConfigLoader.LoadConfig("response_sound_pack.json");
            if (configEntries == null || configEntries.Count == 0)
            {
                BingBongVoiceLineAPI.Log.LogWarning("No custom Bing Bong responses found in config.");
                return;
            }

            List<Action_AskBingBong.BingBongResponse> customResponses = new List<Action_AskBingBong.BingBongResponse>();

            foreach (BingBongResponseConfigEntry entry in configEntries)
            {
                AudioClip clip = manager.GetClip(entry.file);
                if (clip == null)
                {
                    BingBongVoiceLineAPI.Log.LogError($"AudioClip not found for {entry.file}");
                    continue;
                }

                SFX_Instance customSFX = ScriptableObject.CreateInstance<SFX_Instance>();
                customSFX.clips = new[] { clip };

                string subtitleID = entry.file;
                string subtitleText = string.IsNullOrEmpty(entry.subtitle) ? "" : entry.subtitle;
                LocalizationInjector.AddCustomSubtitle(subtitleID, subtitleText);

                Action_AskBingBong.BingBongResponse response = new Action_AskBingBong.BingBongResponse
                {
                    subtitleID = subtitleID,
                    sfx = customSFX,
                    mouthCurve = new AnimationCurve(),
                    mouthCurveTime = 1f
                };
                customResponses.Add(response);
            }

            if (customResponses.Count > 0)
            {
                if (!Configuration.ReplaceBingBongResponses.Value)
                {
                    Action_AskBingBong.BingBongResponse[] combined = latest.responses.Concat(customResponses).ToArray();
                    latest.responses = combined;
                    BingBongVoiceLineAPI.Log.LogInfo($"Combined Bing Bong responses. Total responses: {latest.responses.Length}");
                }
                else
                {
                    latest.responses = customResponses.ToArray();
                    BingBongVoiceLineAPI.Log.LogInfo("Replaced Bing Bong responses with custom responses.");
                }
            }
        }
    }
}
