using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BingBongVoiceLineAPI.Config;
using BingBongVoiceLineAPI.Helpers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BingBongVoiceLineAPI
{
    [BepInPlugin(mod_guid, mod_name, mod_version)]
    public class BingBongVoiceLineAPI : BaseUnityPlugin
    {
        private const string mod_guid = "MrBytesized.PEAK.BingBongVoiceLineAPI";
        private const string mod_name = "Bing Bong Voice Line API";
        private const string mod_version = "1.0.0";

        private readonly Harmony harmony = new Harmony(mod_guid);

        public static BingBongVoiceLineAPI Instance { get; private set; }
        internal static ManualLogSource Log;

        private (ConfigEntry<bool> configEntry, Action enablePatch, Action disablePatch, string description)[] _patchArray;

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            Configuration.Init(Config);

            Log = BepInEx.Logging.Logger.CreateLogSource(mod_guid);

            Log.LogInfo("Bing Bong Voice Line API has been activated");

            if (FindFirstObjectByType<CustomSoundManager>() == null)
            {
                GameObject managerGO = new GameObject("BingBongVoiceLineAPI_AudioManager");
                managerGO.AddComponent<CustomSoundManager>();
                DontDestroyOnLoad(managerGO);
            }

            _patchArray = new (ConfigEntry<bool>, Action, Action, string)[]
            {
                (
                    Configuration.EnableMod,
                    () => 
                    {
                        harmony.PatchAll(typeof(Patches.GameObjectPatch));
                        harmony.PatchAll(typeof(Patches.UnityObjectPatch)); 
                    },
                    () => harmony.UnpatchSelf(),
                    "Bing Bong Voice Line API"
                ),
            };

            foreach (var (configEntry, enablePatch, disablePatch, description) in _patchArray)
            {
                UpdatePatchFromConfig(configEntry, enablePatch, disablePatch, description);
                configEntry.SettingChanged += (sender, args) => UpdatePatchFromConfig(configEntry, enablePatch, disablePatch, description);
            }
        }

        private void UpdatePatchFromConfig(
            ConfigEntry<bool> configEntry,
            Action enablePatch,
            Action disablePatch,
            string description)
        {
            if (configEntry.Value)
            {
                enablePatch.Invoke();
                Log.LogInfo($"{description} patch enabled.");
            }
            else
            {
                disablePatch.Invoke();
                Log.LogInfo($"{description} patch disabled.");
            }
        }
    }
}

