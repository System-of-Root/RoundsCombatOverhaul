using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace RCO.Extensions
{
    public class CharacterData_OverhaulData
    {
        // [Lose Control] status
        public bool isLostControl;
        public float loseControlTimer;

        //

        public float dashTime;
        public Vector2 dashDirection;
        public bool dashedSinceGrounded;

        public bool isGrappled;
        public float grappledTime;

        public CharacterData_OverhaulData()
        {
            isLostControl = false;
            loseControlTimer = 0f;
            dashTime = 0f;
            dashedSinceGrounded = false;
            isGrappled = false;
        }
    }

    public static class CharacterData_Extensions
    {
        public static readonly ConditionalWeakTable<CharacterData, CharacterData_OverhaulData> data =
            new ConditionalWeakTable<CharacterData, CharacterData_OverhaulData>();

        public static CharacterData_OverhaulData GetOverhaulData(this CharacterData characterStat)
        {
            return data.GetOrCreateValue(characterStat);
        }

        public static void AddData(this CharacterData characterData, CharacterData_OverhaulData value)
        {
            try
            {
                data.Add(characterData, value);
            }
            catch (Exception) { }
        }
    }

    // [HarmonyPatch(typeof(CharacterData), "ResetStats")]
    // class CharacterData_Patch_ResetStats
    // {
    //     private static void Prefix(CharacterData __instance)
    //     {
    //         __instance.GetOverhaulData().isLostControl = false;
    //         __instance.GetOverhaulData().loseControlTimer = 0;
    //     }
    // }
}
