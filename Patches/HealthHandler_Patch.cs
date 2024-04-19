using HarmonyLib;
using UnityEngine;
using UnboundLib;

using RCO.Extensions;
using RCO.MonoBehaviours;

// using GearUpCards.Extensions;

namespace RCO.Patches
{
    [HarmonyPatch(typeof(HealthHandler))]
    class HealthHandler_Patch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("RPCA_Die")]
        static void RPCA_Die_Prefix(CharacterData ___data)
        {
            // UnityEngine.Debug.Log($"Player[{___data.player.playerID}] Dying");
            if (___data.isPlaying && !___data.dead)
            {
                Main.playerMonitor.GetComponent<LoseControlMonitor>().watchList.Add(___data.gameObject);
            }
            // UnityEngine.Debug.Log($"Continue Player[{___data.player.playerID}] Dying");
        }

    }
}
