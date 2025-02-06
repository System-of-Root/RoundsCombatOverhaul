using HarmonyLib;
using Photon.Pun;
using RCO.Extensions;
using RCO.VFX;
using System;
using UnboundLib;
using UnityEngine;

namespace RCO.MonoBehaviours
{
    public class RCO_RPCA_Handler : MonoBehaviour
    {
        // MonoBehaviour made purely to receive network executions
        // is this alright? lmao --Pud

        private Player player;
        private CharacterData data;

        public bool RCO_Blocking = false;
        bool RCO_WasBlocking = false;

        private void Start()
        {
            player = GetComponent<Player>();
            data = player.data;
        }

        void Update()
        {
            // manage block visual for non-local player
            if (RCO_Blocking && !RCO_WasBlocking)
            {
                data.block.reloadParticle.Play();
                RCO_WasBlocking = true;
            }
            else if (!RCO_Blocking && RCO_WasBlocking)
            {
                RCO_WasBlocking = false;
            }

            if (RCO_Blocking)
            {
                data.block.sinceBlock = 0f;
                data.block.counter = 0f;
                data.block.reloadParticle.time = 0f;
            }
        }

        [PunRPC]
        public void RPCA_GrappleState(int targetID, bool state)
        {
            Player player = gameObject.GetComponent<Player>();
            if (player.playerID != targetID)
            {
                return;
            }

            player.data.GetOverhaulData().isGrappled = state;
        }

        [PunRPC]
        public void RPCA_TakeForce(int targetID, Vector2 force, int forceMode)
        {
            Player player = gameObject.GetComponent<Player>();
            if (player.playerID != targetID)
            {
                return;
            }

            player.data.playerVel.InvokeMethod("AddForce", new Type[] { typeof(Vector2), typeof(ForceMode2D) }, force, forceMode);
        }

        [PunRPC]
        public void RPCA_DisplayBlock(int playerIDfrom, bool state)
        {
            // should only be execute on non-local player's object!!
            Predicate<Player> check = (item => item.playerID == playerIDfrom);
            Player playerFrom = null;
            if (PlayerManager.instance.players.Exists(check))
            {
                playerFrom = PlayerManager.instance.players.Find(check);
            }
            else
            {
                return;
            }

            RCO_Blocking = state;
        }

        [PunRPC]
        public void RPCA_DisplayHook(int playerIDfrom, Vector3 pos)
        {
            // should only be execute on non-local player's object!!
            Predicate<Player> check = (item => item.playerID == playerIDfrom);
            Player playerFrom = null;
            if (PlayerManager.instance.players.Exists(check))
            {
                playerFrom = PlayerManager.instance.players.Find(check);
            }
            else
            {
                return;
            }

            // spawn hook vfx
            CharacterData data = playerFrom.data;
            PlayerSkinParticle skinParticle = data.gameObject.GetComponentInChildren<PlayerSkinParticle>();
            Color color1 = Traverse.Create(skinParticle).Field("startColor1").GetValue<Color>();
            Color color2 = Traverse.Create(skinParticle).Field("startColor2").GetValue<Color>();

            Gradient gradient = new Gradient();
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
            GradientColorKey[] colorKeys = new GradientColorKey[] { new GradientColorKey(color1, 0.0f), new GradientColorKey(color2, 1.0f) };
            gradient.SetKeys(colorKeys, alphaKeys);

            GameObject vfxObject = GameObject.Instantiate(Main.GrapplingRopePrefab);
            vfxObject.transform.parent = data.weaponHandler.gun.transform;
            vfxObject.transform.localPosition = Vector3.zero;
            vfxObject.GetComponent<LineRenderer>().colorGradient = gradient;

            GrapplingRopeVFX ropeVFX = vfxObject.AddComponent<GrapplingRopeVFX>();
            ropeVFX.targetLastPos = new Vector3(pos.x, pos.y, 0.0f);
            ropeVFX.ropeSnapTime = 0.035f;
            ropeVFX.ropeOvershotTime = 0.035f;
        }
    }
}
