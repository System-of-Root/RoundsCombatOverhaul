using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnityEngine;

namespace RCO.Patches {
    [HarmonyPatch]
    public class DefaultStatChanges {

        //reset the player so i only have to feck with things in one place
        [HarmonyPatch(typeof(Player),"Start")]
        [HarmonyPostfix]
        public static void ResetPlayerOnStart(Player __instance) {
            Unbound.Instance.ExecuteAfterFrames(5, ()=>{ 
                __instance.InvokeMethod("FullReset");
            });
        }

        [HarmonyPatch(typeof(Player), "FullReset")]
        [HarmonyPostfix]
        public static void BasePlayerStats(Player __instance) {
            var gun = __instance.data.weaponHandler.gun;

            gun.GetComponentInChildren<GunAmmo>().maxAmmo = 6;
            gun.GetComponentInChildren<GunAmmo>().ReDrawTotalBullets(); // needed after ammo changes
            gun.projectileSpeed = 2.5f; // bullet physics of Quick Shot
            __instance.data.jumps = 2; //2 jumps
            __instance.data.jump.upForce = 6600; // +20% jump hight/force
            __instance.data.block.cooldown = 0.5f; // half second of block cooldown used as a dash cooldown
        }

        //size locked to 2x normall
        [HarmonyPatch(typeof(CharacterStatModifiers), "ConfigureMassAndSize")]
        [HarmonyPostfix]
        public static void LockSize(CharacterStatModifiers __instance) {
            __instance.transform.localScale = Vector3.one * 1.2f * Mathf.Pow(200f / 100f * 1.2f, 0.2f);
        }


        //block spamming of the jump button
        [HarmonyPatch(typeof(PlayerJump), "Jump")]
        [HarmonyPrefix]
        public static bool SlowJump(PlayerJump __instance) {
            return ((CharacterData)__instance.GetFieldValue("data")).sinceJump > 0.1f;
        }


    }
}
