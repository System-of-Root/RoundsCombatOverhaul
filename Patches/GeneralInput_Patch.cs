using HarmonyLib;
using UnityEngine;
using UnboundLib;

using RCO.Extensions;

// using GearUpCards.Extensions;

namespace RCO.Patches
{
    [HarmonyPatch(typeof(GeneralInput))]
    class GeneralInput_Patch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("Update")]
        static bool Update_Prefix(CharacterData ___data, GeneralInput __instance)
        {
            if (___data.GetOverhaulData().isLostControl)
            {
                try
                {
                    __instance.ResetInput();
                    // UnityEngine.Debug.LogWarning("GeneralInput::ResetInput()");
                    __instance.InvokeMethod("DoUIInput");
                    // UnityEngine.Debug.LogWarning("GeneralInput::DoUIInput()");
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning("GeneralInput::Update_Prefix : ResetInput failed");
                    UnityEngine.Debug.LogWarning($"{e.Message}");
                }
                return false;
            }

            return true;
        }


    }
}