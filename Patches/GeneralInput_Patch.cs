using HarmonyLib;
using UnityEngine;
using UnboundLib;

using RCO.Extensions;
using RCO.MonoBehaviours;
using Photon.Pun;
using System;

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
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("Update")]
        static void Update_Postfix(CharacterData ___data, GeneralInput __instance) {

            Vector2 aim = new Vector2(___data.playerActions.Aim.X, ___data.playerActions.Aim.Y);
            Vector2 move = new Vector2(___data.playerActions.Move.X, ___data.playerActions.Move.Y);

            if(__instance.shootIsPressed) {
                __instance.ResetInput();
                __instance.aimDirection = aim;
                return;
            }

            if(__instance.shootWasReleased) {
                //graple logic

            }

            __instance.shootWasPressed = false;
            __instance.shootIsPressed = false;
            __instance.shootWasReleased = false;

            if(___data.GetOverhaulData().dashTime > 0f) {
                ___data.GetOverhaulData().dashTime -= TimeHandler.deltaTime;
                ___data.sinceGrounded = 0;
                ___data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) }, ___data.GetOverhaulData().dashDirection * TimeHandler.deltaTime * 4400000, ForceMode2D.Force);
                __instance.direction = Vector3.zero;
                ___data.block.counter = 0f;
            } else {
                ___data.GetOverhaulData().dashedSinceGrounded = ___data.GetOverhaulData().dashedSinceGrounded || ___data.isGrounded;
            }
            ___data.GetComponent<Gravity>().enabled = ___data.GetOverhaulData().dashTime <= 0f;

            if(aim.magnitude > 0.6f) {
                __instance.aimDirection = aim;
                __instance.shootWasPressed = true;
                ___data.weaponHandler.gun.shootPosition.rotation = Quaternion.LookRotation(aim);
                ___data.weaponHandler.InvokeMethod("Attack");
            }
            if(___data.isGrounded && ___data.playerActions.Block.IsPressed && move.magnitude < 0.1f && !__instance.jumpIsPressed && (!___data.block.IsOnCD() || ___data.block.counter <= TimeHandler.deltaTime)) {
                ___data.block.sinceBlock = 0f;
                ___data.block.counter = 0f;
                ___data.block.reloadParticle.Play(); 
                ___data.block.reloadParticle.time = 0f;
            }
            if(___data.block.reloadParticle.time > 0 && ___data.block.reloadParticle.isPlaying) {
                LoseControlHandler handler = ___data.player.gameObject.GetOrAddComponent<LoseControlHandler>();
                ___data.view.RPC("RPCA_AddLoseControl", RpcTarget.All, 0.5f);
            }
            if(___data.playerActions.Block.IsPressed && move.magnitude > 0.1f && !___data.block.IsOnCD() && ___data.GetOverhaulData().dashedSinceGrounded) {
                ___data.block.counter = 0f;
                ___data.GetOverhaulData().dashTime = 0.15f;
                ___data.GetOverhaulData().dashDirection = move.normalized;
                ___data.GetOverhaulData().dashedSinceGrounded = false;
            }
        }


    }
}