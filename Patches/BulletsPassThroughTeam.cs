using HarmonyLib;
using RCO.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RCO.Patches {
    [HarmonyPatch]
    public class BulletsPassThroughTeam {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProjectileHit), nameof(ProjectileHit.Hit))]
        public static bool ProjectileHitPrefixHit(HitInfo hit, ProjectileHit __instance) {
            if((bool)hit.transform) {
                HealthHandler healthHandler = hit.transform.GetComponent<HealthHandler>();
                if(healthHandler == null) return true;
                return (healthHandler.GetComponent<Player>().teamID != __instance.ownPlayer.teamID && healthHandler.GetComponent<CharacterData>().GetOverhaulData().dashTime <= 0);
            }
            return true;
        }

    }
}
