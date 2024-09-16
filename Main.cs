using BepInEx;
using HarmonyLib;
using RCO.Dev;
using RCO.MonoBehaviours;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.Utils;
using UnityEngine;

namespace RCO {
    [BepInDependency("com.willis.rounds.unbound")]

    [BepInProcess("Rounds.exe")]

    [BepInPlugin(ModId, ModName, Version)]
    public class Main:BaseUnityPlugin {
        public static GameObject playerMonitor;
        /**/public static GameObject playerObj;

        public const string ModId = "com.roots.rounds.RoundsCombatOverhaul", 
            ModName = "RoundsCombatOverhaul", 
            Version = "0.0.10";

        public static readonly AssetBundle RCOAsset = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("rco_asset", typeof(Main).Assembly);
        public static GameObject GrapplingRopePrefab = RCOAsset.LoadAsset<GameObject>("GrapplingSource");

        void Awake() {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }

        void Start() {
            LevelManager.RegisterMaps(Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("rco_maps", typeof(Main).Assembly), "RCO");
            playerMonitor = new GameObject("[RCO] PlayerMonitor");
            playerMonitor.AddComponent<LoseControlMonitor>();
            DontDestroyOnLoad(playerMonitor);

            this.ExecuteAfterFrames(5, () => {
                List<string> categories = (List<string>)typeof(LevelManager)
                .GetField("categories", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .GetValue(null);
                categories.ForEach(c => { if(c != "RCO") LevelManager.DisableCategory(c); });
            });

            this.ExecuteAfterFrames(95, () =>
            {
                GameObject[] gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
                foreach (var item in gameObjects)
                {
                    if (item.name == "Player" && item.CompareTag("Player"))
                    {
                        playerObj = item;
                        break;
                    }
                }

                GeneralInput generalInput = playerObj.GetComponent<GeneralInput>();
                SilenceHandler silenceHandler = playerObj.GetComponent<SilenceHandler>();

                bool check = (generalInput != null) && (silenceHandler != null);

                if (check)
                {
                    playerObj.AddComponent<LoseControlHandler>();
                    UnityEngine.Debug.Log("[RCO] added [LoseControlHandler] to the original Player object");
                }
            });
            
            // dev card, DO NOT INCLUDE IN FINAL BUILD!
            //CustomCard.BuildCard<LoseControlDevCard>();
        }

        void Update(){
            
        }
    }
}
