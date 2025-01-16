using BepInEx;
using HarmonyLib;
using RCO.Dev;
using RCO.MonoBehaviours;
using RCO.VFX;
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
            Version = "0.0.15";

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
                    LoseControlHandler handler = playerObj.AddComponent<LoseControlHandler>();
                    UnityEngine.Debug.Log("[RCO] added [LoseControlHandler] to the original Player object");

                    GameObject loseControlVFX = RCOAsset.LoadAsset<GameObject>("VFX_Confused");
                    GameObject disarmedVFX = RCOAsset.LoadAsset<GameObject>("VFX_Disarmed");

                    // adding [Lose Control] VFX
                    UnityEngine.Debug.Log("[RCO] VFX A");
                    GameObject parentObj = new GameObject("LoseControl_VFX");
                    parentObj.transform.parent = playerObj.transform;
                    parentObj.transform.localPosition = Vector3.zero;

                    GameObject vfxObject = GameObject.Instantiate(loseControlVFX);
                    vfxObject.transform.parent = parentObj.transform;
                    vfxObject.transform.localPosition = Vector3.zero + new Vector3(0.0f, -1.0f, 0.0f);
                    vfxObject.transform.localScale = Vector3.one * 2.0f;
                    vfxObject.SetActive(false);

                    handler.LoseControlVFX = parentObj.AddComponent<SimpleStatusVFX>();
                    handler.LoseControlVFX.targetObject = vfxObject;

                    // adding [Disarmed]
                    UnityEngine.Debug.Log("[RCO] VFX B");
                    parentObj = new GameObject("Disarmed_VFX");
                    parentObj.transform.parent = playerObj.transform;
                    parentObj.transform.localPosition = Vector3.zero;

                    vfxObject = GameObject.Instantiate(disarmedVFX);
                    vfxObject.transform.parent = parentObj.transform;
                    vfxObject.transform.localPosition = Vector3.zero + new Vector3(0.0f, 1.5f, 0.0f);
                    vfxObject.transform.localScale = Vector3.one * 2.0f;
                    vfxObject.SetActive(false);

                    handler.DisarmedVFX = parentObj.AddComponent<SimpleStatusVFX>();
                    handler.DisarmedVFX.targetObject = vfxObject;

                    UnityEngine.Debug.Log("[RCO] VFX Done!");
                }
            });
            
            // dev card, DO NOT INCLUDE IN FINAL BUILD!
            // CustomCard.BuildCard<LoseControlDevCard>();
        }

        void Update(){
            
        }
    }
}
