using HarmonyLib;
using RCO.Extensions;
using UnityEngine;

namespace RCO.Patches {

    [HarmonyPatch(typeof(PlayerVelocity), "FixedUpdate")]
    public class PlayerVelocityPatch {

        public static void Prefix(CharacterData ___data, bool ___simulated, bool ___isKinematic, ref Vector2 ___velocity, PlayerVelocity __instance) {
            if(___simulated && !___isKinematic && ___data.isPlaying && (___data.GetOverhaulData().dashTime >= 0 || ___data.GetOverhaulData().isGrappled)) 
                ___velocity += Vector2.up* Time.fixedDeltaTime * TimeHandler.timeScale* 20f;
        }
    }
}
