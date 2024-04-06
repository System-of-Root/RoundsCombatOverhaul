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

        }

        void Start() {

        }
    }
}
