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
    [HarmonyPatch(typeof(UnityEngine.Object))]
    static class UnityObjectPatch
    {
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object) })]
        static void Postfix(UnityEngine.Object __result)
        {
            try
            {
                BingBongVoiceLineReplacer.TryHandleCreatedObject(__result);
            }
            catch (Exception ex)
            {
                BingBongVoiceLineAPI.Log.LogError($"[UnityObjectPatch] Instantiate_Postfix exception: {ex}");
            }
        }
    }
}
