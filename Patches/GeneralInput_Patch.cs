using HarmonyLib;
using Photon.Pun;
using RCO.Extensions;
using RCO.MonoBehaviours;
using RCO.VFX;
using System;
using System.Collections;
using UnboundLib;
using UnityEngine;

namespace RCO.Patches {
    [HarmonyPatch(typeof(GeneralInput))]
    class GeneralInput_Patch {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("Update")]
        static bool Update_Prefix(CharacterData ___data, GeneralInput __instance) {
            if (__instance.controlledElseWhere) {
                // UnityEngine.Debug.Log($"[RCO] Prefix -- Player[{___data.player.playerID}] is non-local player");
                return true;
            }

            if (___data.GetOverhaulData().isLostControl) {
                try {
                    __instance.ResetInput();
                    __instance.InvokeMethod("DoUIInput");
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
            // UnityEngine.Debug.Log($"[RCO] Postfix -- Player[{___data.player.playerID}]");
            if (__instance.controlledElseWhere) {
                // UnityEngine.Debug.Log($"[RCO] Postfix -- Player[{___data.player.playerID}] is non-local player");
                return;
            }

            bool grapleWasPressed = __instance.inputType == GeneralInput.InputType.Controller ? __instance.shootIsPressed && !__instance.shootWasReleased 
                : __instance.GetComponent<CharacterData>().playerActions["Grapple"].IsPressed && !__instance.GetComponent<CharacterData>().playerActions["Grapple"].WasReleased;
            bool grapleWasReleased = __instance.inputType == GeneralInput.InputType.Controller ? __instance.shootWasReleased 
                : __instance.GetComponent<CharacterData>().playerActions["Grapple"].WasReleased;

            ___data.GetOverhaulData().grapleWasPressed = grapleWasPressed;
            ___data.GetOverhaulData().grapleWasReleased = grapleWasReleased;
            // UnityEngine.Debug.Log("[RCO] Postfix -- Grapple button prev-state");

            //Handle control-related status effects
            if (___data.GetOverhaulData().isImmobile)
            {
                __instance.direction = Vector3.zero;

                __instance.jumpIsPressed = false;
                __instance.jumpWasPressed = false;
            }

            if (___data.GetOverhaulData().isDisarmed)
            {
                __instance.shootIsPressed = false;
                __instance.shootWasPressed = false;
                __instance.shootWasReleased = false;
            }
            // UnityEngine.Debug.Log("[RCO] Postfix -- Control status effect");

            //Handle Dash Movement
            if (___data.GetOverhaulData().dashTime > 0f) {
                ___data.GetOverhaulData().dashTime -= TimeHandler.deltaTime;
                ___data.sinceGrounded = 0.05f;
                ___data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) },
                    ___data.GetOverhaulData().dashDirection * TimeHandler.deltaTime * 8800000, ForceMode2D.Force);
                __instance.direction = Vector3.zero;
                ___data.block.counter = 0f;
            } else {
                ___data.GetOverhaulData().groundedSinceDash = ___data.GetOverhaulData().groundedSinceDash || ___data.isGrounded;
            }
            // UnityEngine.Debug.Log("[RCO] Postfix -- Dash Movement");

            //Diable Gavity for dash and when grappled
            ___data.GetComponent<Gravity>().enabled = ___data.GetOverhaulData().dashTime <= 0f && !___data.GetOverhaulData().isGrappled;

            if(___data.GetOverhaulData().isThrowing) {
                ___data.GetComponent<PlayerVelocity>().enabled = false;
                ___data.GetComponent<Gravity>().enabled = false;
            }
            else {
                ___data.GetComponent<PlayerVelocity>().enabled = true;
                ___data.GetComponent<Gravity>().enabled = true;
            }
            // UnityEngine.Debug.Log("[RCO] Postfix -- Grapple and gravity");

            Vector2 aim = new Vector2(___data.playerActions.Aim.X, ___data.playerActions.Aim.Y);
            Vector2 move = new Vector2(___data.playerActions.Move.X, ___data.playerActions.Move.Y);


            //Start grapple action
            if(grapleWasPressed && ___data.GetOverhaulData().groundedSinceGrapple && !___data.GetOverhaulData().isGrappling) {
                __instance.ResetInput();
                __instance.aimDirection = aim;
                // ___data.GetOverhaulData().isGrappling = true;
                return;
            }
            
            //preform grapple
            if(grapleWasReleased && ___data.GetOverhaulData().groundedSinceGrapple && !___data.GetOverhaulData().isGrappling) {
                ___data.GetOverhaulData().isGrappling = true;
                ___data.GetOverhaulData().groundedSinceGrapple = ___data.isGrounded;___data.isGrounded = false;
                Unbound.Instance.ExecuteAfterSeconds(1.5f, () => { ___data.GetOverhaulData().isGrappling = false; });

                float maxLangth = MainCam.instance.cam.orthographicSize;
                int layerMask = (1 << 0)| (1 << 11) | (1 << 10);
                RaycastHit2D hit = Physics2D.Raycast(___data.weaponHandler.gun.transform.position, __instance.lastAimDirection, maxLangth, layerMask); ;
                if(hit.collider != null) UnityEngine.Debug.Log(hit.collider.gameObject.name);


                if(hit.collider == null || hit.collider.gameObject.layer == 0) {  //Miss and get [Disarmed]
                    ___data.player.gameObject.GetOrAddComponent<LoseControlHandler>();
                    Unbound.Instance.ExecuteAfterSeconds(0.2f, () => {
                        ___data.view.RPC("RPCA_AddDisarmed", RpcTarget.All, 0.8f);
                    });
                    
                    // Spawn Grappling VFX
                    PlayerSkinParticle skinParticle = ___data.gameObject.GetComponentInChildren<PlayerSkinParticle>();
                    Color color1 = Traverse.Create(skinParticle).Field("startColor1").GetValue<Color>();
                    Color color2 = Traverse.Create(skinParticle).Field("startColor2").GetValue<Color>();

                    Gradient gradient = new Gradient();
                    GradientAlphaKey[] alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
                    GradientColorKey[] colorKeys = new GradientColorKey[] { new GradientColorKey(color1, 0.0f), new GradientColorKey(color2, 1.0f) };
                    gradient.SetKeys(colorKeys, alphaKeys);

                    GameObject vfxObject = GameObject.Instantiate(Main.GrapplingRopePrefab);
                    vfxObject.transform.parent = ___data.weaponHandler.gun.transform;
                    vfxObject.transform.localPosition = Vector3.zero;
                    vfxObject.GetComponent<LineRenderer>().colorGradient = gradient;

                    GrapplingRopeVFX ropeVFX = vfxObject.AddComponent<GrapplingRopeVFX>();
                    Vector3 endPoint;
                    if(hit.collider != null) {
                        endPoint = hit.point;
                    }
                    else {
                        endPoint = vfxObject.transform.position + (new Vector3(__instance.lastAimDirection.x, __instance.lastAimDirection.y, 0.0f).normalized * maxLangth);
                    }
                    ropeVFX.targetLastPos = endPoint;

                    // display hook animation on other clients
                    ___data.view.RPC("RPCA_DisplayHook", RpcTarget.Others, ___data.player.playerID, endPoint);
                } 
                else if(hit.collider.GetComponent<Player>() != null) { //Grapple Player
                    Unbound.Instance.ExecuteAfterSeconds(1.1f, () => { ___data.GetOverhaulData().isThrowing = false; });

                    ___data.sinceGrounded = 0.05f;
                    Player targetPlayer = hit.collider.GetComponent<Player>();
                    {//pull player in and then throw
                        Unbound.Instance.ExecuteAfterSeconds(0.1f, () => {
                            Vector2 force = __instance.transform.position - hit.collider.transform.position;
                            // targetPlayer.data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) }, force * 2750, ForceMode2D.Impulse);
                            targetPlayer.data.view.RPC("RPCA_TakeForce", RpcTarget.All, targetPlayer.data.player.playerID, force * 2750, (int)(ForceMode2D.Impulse));
                            targetPlayer.data.view.RPC("RPCA_AddLoseControl", RpcTarget.All, 0.1f);
                        });

                        // lock down players
                        Unbound.Instance.ExecuteAfterSeconds(0.175f, () => {
                            targetPlayer.data.view.RPC("RPCA_SetStun", RpcTarget.All, 0.825f);
                            // targetPlayer.data.GetOverhaulData().isGrappled = true;
                            targetPlayer.data.view.RPC("RPCA_GrappleState", RpcTarget.All, targetPlayer.data.player.playerID, true);

                            ___data.view.RPC("RPCA_SetImmobile", RpcTarget.All, 0.825f);
                            ___data.view.RPC("RPCA_SetDisarm", RpcTarget.All, 0.825f);
                            ___data.GetOverhaulData().isThrowing = true;
                        });

                        // throw coroutine
                        Unbound.Instance.StartCoroutine(DoGrapple(___data.player, targetPlayer));

                    }

                    // Spawn Grappling VFX
                    PlayerSkinParticle skinParticle = ___data.gameObject.GetComponentInChildren<PlayerSkinParticle>();
                    Color color1 = Traverse.Create(skinParticle).Field("startColor1").GetValue<Color>();
                    Color color2 = Traverse.Create(skinParticle).Field("startColor2").GetValue<Color>();

                    Gradient gradient = new Gradient();
                    GradientAlphaKey[] alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
                    GradientColorKey[] colorKeys = new GradientColorKey[] { new GradientColorKey(color1, 0.0f), new GradientColorKey(color2, 1.0f) };
                    gradient.SetKeys(colorKeys, alphaKeys);

                    GameObject vfxObject = GameObject.Instantiate(Main.GrapplingRopePrefab);
                    vfxObject.transform.parent = ___data.weaponHandler.gun.transform;
                    vfxObject.transform.localPosition = Vector3.zero;
                    vfxObject.GetComponent<LineRenderer>().colorGradient = gradient;

                    GrapplingRopeVFX ropeVFX = vfxObject.AddComponent<GrapplingRopeVFX>();
                    ropeVFX.targetObject = targetPlayer.gameObject;
                    ropeVFX.ropeSnapTime = 0.035f;
                    ropeVFX.ropeOvershotTime = 0.035f;

                    // display hook animation on other clients
                    ___data.view.RPC("RPCA_DisplayHook", RpcTarget.Others, ___data.player.playerID, targetPlayer.gameObject.transform.position);

                } else { //Grapple Map Point
                    ___data.sinceGrounded = 0.05f;
                    Unbound.Instance.ExecuteAfterSeconds(0.1f, () => {
                        Vector2 force = hit.collider.transform.position - __instance.transform.position;
                        ___data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) }, force * 2750, ForceMode2D.Impulse);
                    });

                    // Spawn Grappling VFX
                    PlayerSkinParticle skinParticle = ___data.gameObject.GetComponentInChildren<PlayerSkinParticle>();
                    Color color1 = Traverse.Create(skinParticle).Field("startColor1").GetValue<Color>();
                    Color color2 = Traverse.Create(skinParticle).Field("startColor2").GetValue<Color>();

                    Gradient gradient = new Gradient();
                    GradientAlphaKey[] alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
                    GradientColorKey[] colorKeys = new GradientColorKey[] { new GradientColorKey(color1, 0.0f), new GradientColorKey(color2, 1.0f) };
                    gradient.SetKeys(colorKeys, alphaKeys);
                    
                    GameObject vfxObject = GameObject.Instantiate(Main.GrapplingRopePrefab);
                    vfxObject.transform.parent = ___data.weaponHandler.gun.transform;
                    vfxObject.transform.localPosition = Vector3.zero;
                    vfxObject.GetComponent<LineRenderer>().colorGradient = gradient;

                    GrapplingRopeVFX ropeVFX = vfxObject.AddComponent<GrapplingRopeVFX>();
                    ropeVFX.targetObject = hit.collider.gameObject;

                    // display hook animation on other clients
                    ___data.view.RPC("RPCA_DisplayHook", RpcTarget.Others, ___data.player.playerID, hit.collider.gameObject.transform.position);
                }


            } else {
                ___data.GetOverhaulData().groundedSinceGrapple = ___data.GetOverhaulData().groundedSinceGrapple || ___data.isGrounded;
            }
            // UnityEngine.Debug.Log("[RCO] Postfix -- Grapple Action block");

            if (__instance.jumpWasPressed) ___data.GetOverhaulData().isGrappling = false; //exit grapple early

            if(__instance.inputType == GeneralInput.InputType.Controller) {
                //reset gun data used for grapple so the player doesnt fire
                __instance.shootWasPressed = false;
                __instance.shootIsPressed = false;
                __instance.shootWasReleased = false;

                //shoot with aim stick if not grapled and not [Disarmed]
                bool canShootCheck = !___data.GetOverhaulData().isGrappling &&
                                     !___data.GetOverhaulData().isDisarmed &&
                                     !___data.GetOverhaulData().isLostControl &&
                                     !___data.isStunned &&
                                     !___data.isSilenced;

                if(aim.magnitude > 0.6f && canShootCheck) {
                    __instance.aimDirection = aim;
                    __instance.shootWasPressed = true;
                    ___data.weaponHandler.gun.shootPosition.rotation = Quaternion.LookRotation(aim);
                    ___data.weaponHandler.InvokeMethod("Attack");
                }
            }

            //hold block if stationary, disable gun
            if(___data.isGrounded && ___data.playerActions.Block.IsPressed && move.magnitude < 0.1f &&
                !__instance.jumpIsPressed && (!___data.block.IsOnCD() || ___data.block.counter <= TimeHandler.deltaTime
                && !___data.GetOverhaulData().isGrappling)) {
                ___data.block.sinceBlock = 0f;
                ___data.block.counter = 0f;
                ___data.block.reloadParticle.Play();
                ___data.block.reloadParticle.time = 0f;
                ___data.view.RPC("RPCA_AddDisarmed", RpcTarget.All, 0.5f);

                // send block state-ON (visually)
                ___data.view.RPC("RPCA_DisplayBlock", RpcTarget.Others, ___data.player.playerID, true);
            }

            //disarm player when they stop blocking
            if(___data.block.reloadParticle.time > 0 && ___data.block.reloadParticle.isPlaying) {
                ___data.player.gameObject.GetOrAddComponent<LoseControlHandler>();
                ___data.view.RPC("RPCA_AddDisarmed", RpcTarget.All, 0.5f);

                // send block state-OFF (visually)
                ___data.view.RPC("RPCA_DisplayBlock", RpcTarget.Others, ___data.player.playerID, false);
            }

            //start dash if blocking while moving and not [Immobile]'d.
            if(___data.playerActions.Block.IsPressed && move.magnitude > 0.1f && !___data.block.IsOnCD() && ___data.GetOverhaulData().groundedSinceDash && !___data.GetOverhaulData().isImmobile) {
                ___data.block.counter = 0f;
                ___data.GetOverhaulData().dashTime = 0.1f;
                ___data.GetOverhaulData().dashDirection = move.normalized;
                ___data.GetOverhaulData().groundedSinceDash = ___data.isGrounded; ___data.isGrounded = false;
                ___data.block.particle.Play();
            }
            // UnityEngine.Debug.Log("[RCO] Postfix -- RCO Blocking");
        }

        public static IEnumerator DoGrapple(Player Holder, Player Gappled) {
            //Holding on enemy player and throwing
            float time = 0;
            do {
                yield return null;
                time += TimeHandler.deltaTime;
                if(Holder.data.GetOverhaulData().grapleWasPressed && time > 0.2f) {
                    //Throwing enemy player
                    Vector2 move = Holder.data.input.inputType == GeneralInput.InputType.Controller ? new Vector2(Holder.data.playerActions.Move.X, Holder.data.playerActions.Move.Y) 
                        : (Vector2)Holder.data.input.lastAimDirection;

                    //player states -- getting thrown
                    // Gappled.data.GetOverhaulData().isGrappled = false;
                    Gappled.data.view.RPC("RPCA_GrappleState", RpcTarget.All, Gappled.data.player.playerID, false);

                    Gappled.gameObject.GetOrAddComponent<LoseControlHandler>();

                    //status effects
                    Gappled.data.view.RPC("RPCA_SetStun", RpcTarget.All, 0.0f);
                    Gappled.data.view.RPC("RPCA_AddLoseControl", RpcTarget.All, 0.4f);

                    Holder.data.view.RPC("RPCA_SetImmobile", RpcTarget.All, 0.0f);
                    Holder.data.view.RPC("RPCA_SetDisarm", RpcTarget.All, 0.05f);
                    Holder.data.GetOverhaulData().isThrowing = false;

                    //apply force
                    // Gappled.data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) }, move.normalized * 30000, ForceMode2D.Impulse);
                    Gappled.data.view.RPC("RPCA_TakeForce", RpcTarget.All, Gappled.data.player.playerID, move.normalized * 30000, (int)(ForceMode2D.Impulse));

                    yield break;
                }

            } while(time <= 1|| !Holder.data.GetOverhaulData().isGrappling);

            //player states -- not-throwing
            // Gappled.data.GetOverhaulData().isGrappled = false;
            Gappled.data.view.RPC("RPCA_GrappleState", RpcTarget.All, Gappled.data.player.playerID, false);
            Holder.data.GetOverhaulData().isThrowing = false;

            yield break;
        }

    }
}