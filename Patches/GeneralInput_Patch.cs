using HarmonyLib;
using UnityEngine;
using UnboundLib;

using RCO.Extensions;
using RCO.MonoBehaviours;
using Photon.Pun;
using System;

// using GearUpCards.Extensions;

namespace RCO.Patches {
    [HarmonyPatch(typeof(GeneralInput))]
    class GeneralInput_Patch {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("Update")]
        static bool Update_Prefix(CharacterData ___data, GeneralInput __instance) {
            if(___data.GetOverhaulData().isLostControl) {
                try {
                    __instance.ResetInput();
                    // UnityEngine.Debug.LogWarning("GeneralInput::ResetInput()");
                    __instance.InvokeMethod("DoUIInput");
                    // UnityEngine.Debug.LogWarning("GeneralInput::DoUIInput()");
                } catch(System.Exception e) {
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
            if(___data.GetOverhaulData().dashTime > 0f) {
                ___data.GetOverhaulData().dashTime -= TimeHandler.deltaTime;
                ___data.sinceGrounded = 0;
                ___data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) }, ___data.GetOverhaulData().dashDirection * TimeHandler.deltaTime * 6600000, ForceMode2D.Force);
                __instance.direction = Vector3.zero;
                ___data.block.counter = 0f;
            } else {
                ___data.GetOverhaulData().groundedSinceDash = ___data.GetOverhaulData().groundedSinceDash || ___data.isGrounded;
            }

            ___data.GetComponent<Gravity>().enabled = ___data.GetOverhaulData().dashTime <= 0f && !___data.GetOverhaulData().isGrappled;

            Vector2 aim = new Vector2(___data.playerActions.Aim.X, ___data.playerActions.Aim.Y);
            Vector2 move = new Vector2(___data.playerActions.Move.X, ___data.playerActions.Move.Y);

            if(__instance.shootIsPressed && !__instance.shootWasReleased && ___data.GetOverhaulData().groundedSinceGrapple) {
                __instance.ResetInput();
                __instance.aimDirection = aim;
                ___data.GetOverhaulData().isGrappling = true;
                return;
            }

            if(__instance.shootWasReleased && ___data.GetOverhaulData().groundedSinceGrapple) {
                ___data.GetOverhaulData().groundedSinceDash = ___data.isGrounded;
                Unbound.Instance.ExecuteAfterSeconds(2, () => { ___data.GetOverhaulData().isGrappling = false; });
                float maxLangth = MainCam.instance.cam.orthographicSize;
                int layerMask = (1 << 11) | (1 << 10);
                RaycastHit2D hit = Physics2D.Raycast(___data.weaponHandler.gun.transform.position, aim, maxLangth, layerMask);
                if(hit.collider != null) UnityEngine.Debug.Log(hit.collider.gameObject.name);
                if(hit.collider == null) {
                    ___data.player.gameObject.GetOrAddComponent<LoseControlHandler>();
                    ___data.view.RPC("RPCA_AddLoseControl", RpcTarget.All, 0.75f);
                } else if(hit.collider.GetComponent<Player>() != null) {
                    Player player = hit.collider.GetComponent<Player>();
                    player.data.GetOverhaulData().isGrappled = true;
                    if(move.magnitude < 0.1f) {
                        Unbound.Instance.ExecuteAfterSeconds(0.1f, () => {
                            Vector2 force = hit.collider.transform.position - __instance.transform.position;
                            ___data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) }, force * 2750, ForceMode2D.Impulse);
                        });
                        Unbound.Instance.ExecuteAfterSeconds(0.17f, () => {
                            player.data.GetOverhaulData().isGrappled = false;
                            player.gameObject.GetOrAddComponent<LoseControlHandler>();
                            player.data.view.RPC("RPCA_AddLoseControl", RpcTarget.All, 0.1f);
                        });
                    } else {
                        Unbound.Instance.ExecuteAfterSeconds(0.1f, () => {
                            Vector2 force = __instance.transform.position - hit.collider.transform.position;
                            player.data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) }, force * 2750, ForceMode2D.Impulse);
                        });

                        Unbound.Instance.ExecuteAfterSeconds(0.17f, () => {
                            player.data.GetOverhaulData().isGrappled = false;
                            player.gameObject.GetOrAddComponent<LoseControlHandler>();
                            player.data.view.RPC("RPCA_AddLoseControl", RpcTarget.All, 0.4f);
                            player.data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) }, move.normalized * 30000, ForceMode2D.Impulse);
                        });

                    }
                } else {
                    Unbound.Instance.ExecuteAfterSeconds(0.1f, () => {
                        Vector2 force = hit.collider.transform.position - __instance.transform.position;
                        ___data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) }, force * 2750, ForceMode2D.Impulse);
                    });
                }


            } else {
                ___data.GetOverhaulData().groundedSinceGrapple = ___data.GetOverhaulData().groundedSinceGrapple || ___data.isGrounded;
            }

            if(__instance.jumpWasPressed) ___data.GetOverhaulData().isGrappling = false;

            __instance.shootWasPressed = false;
            __instance.shootIsPressed = false;
            __instance.shootWasReleased = false;


            if(aim.magnitude > 0.6f && !___data.GetOverhaulData().isGrappling) {
                __instance.aimDirection = aim;
                __instance.shootWasPressed = true;
                ___data.weaponHandler.gun.shootPosition.rotation = Quaternion.LookRotation(aim);
                ___data.weaponHandler.InvokeMethod("Attack");
            }
            if(___data.isGrounded && ___data.playerActions.Block.IsPressed && move.magnitude < 0.1f &&
                !__instance.jumpIsPressed && (!___data.block.IsOnCD() || ___data.block.counter <= TimeHandler.deltaTime
                && !___data.GetOverhaulData().isGrappling)) {
                ___data.block.sinceBlock = 0f;
                ___data.block.counter = 0f;
                ___data.block.reloadParticle.Play();
                ___data.block.reloadParticle.time = 0f;
            }
            if(___data.block.reloadParticle.time > 0 && ___data.block.reloadParticle.isPlaying) {
                ___data.player.gameObject.GetOrAddComponent<LoseControlHandler>();
                ___data.view.RPC("RPCA_AddLoseControl", RpcTarget.All, 0.5f);
            }
            if(___data.playerActions.Block.IsPressed && move.magnitude > 0.1f && !___data.block.IsOnCD() && ___data.GetOverhaulData().groundedSinceDash) {
                ___data.block.counter = 0f;
                ___data.GetOverhaulData().dashTime = 0.1f;
                ___data.GetOverhaulData().dashDirection = move.normalized;
                ___data.GetOverhaulData().groundedSinceDash = ___data.isGrounded;
            }
        }


    }
}