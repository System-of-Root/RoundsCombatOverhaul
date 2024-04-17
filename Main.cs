using BepInEx;
using HarmonyLib;
using RCO.Dev;
using RCO.MonoBehaviours;
using UnboundLib.Cards;
using UnityEngine;

namespace RCO {
    [BepInDependency("com.willis.rounds.unbound")]

    [BepInProcess("Rounds.exe")]

    [BepInPlugin(ModId, ModName, Version)]
    public class Main:BaseUnityPlugin {
        public static GameObject playerMonitor;

        public const string ModId = "com.roots.rounds.RoundsCombatOverhaul", 
            ModName = "RoundsCombatOverhaul", 
            Version = "0.0.6";

        void Awake() {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }

        void Start() {
            playerMonitor = new GameObject("[RCO] PlayerMonitor");
            playerMonitor.AddComponent<LoseControlMonitor>();
            DontDestroyOnLoad(playerMonitor);

            // dev card, DO NOT INCLUDE IN FINAL BUILD!
            CustomCard.BuildCard<LoseControlDevCard>();
        }

        void Update(){
            
        }
    }
}
