using BepInEx;

namespace RCO {
    [BepInDependency("com.willis.rounds.unbound")]

    [BepInProcess("Rounds.exe")]

    [BepInPlugin(ModId, ModName, Version)]
    public class Main:BaseUnityPlugin {
        public const string ModId = "", 
            ModName = "", 
            Version = "0.0.0";


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
