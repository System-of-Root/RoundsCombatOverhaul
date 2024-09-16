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

        // [Disarmed] status
        public bool isDisarmed;
        public float disarmedTimer;

        // [Immobile] status
        public bool isImmobile;
        public float immobileTimer;

        // variables for Dashing and Grappling
        public float dashTime;
        public Vector2 dashDirection;
        public bool groundedSinceDash;

        public bool isGrappled;
        public bool isGrappling;
        public bool groundedSinceGrapple;

        public CharacterData_OverhaulData()
        {
            isLostControl = false;
            loseControlTimer = 0f;

            isDisarmed = false;
            disarmedTimer = 0f;

            isImmobile = false;
            immobileTimer = 0f;

            dashTime = 0f;
            groundedSinceDash = true;

            isGrappled = false;
            isGrappling = false;
            groundedSinceGrapple = true;
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
