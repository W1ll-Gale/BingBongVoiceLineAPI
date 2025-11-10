using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BingBongVoiceLineAPI.Patches
{
    [HarmonyPatch(typeof(GameObject))]
    static class GameObjectPatch
    {
        [HarmonyPatch(nameof(GameObject.AddComponent), new Type[] { typeof(Type) })]
        internal static void Postfix(GameObject __instance, ref Component __result)
        {
            try
            {
                BingBongVoiceLineReplacer.TryHandleCreatedObject(__result ?? (UnityEngine.Object)__instance);
            }
            catch (Exception ex)
            {
                BingBongVoiceLineAPI.Log.LogError($"[GameObjectPatch] AddComponent_Postfix exception: {ex}");
            }
        }
    }
}
