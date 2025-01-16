using HarmonyLib;
using InControl;
using UnboundLib;

namespace RCO.Patches {
    [HarmonyPatch(typeof(PlayerActions),"CreateWithControllerBindings")]
    public class PlayerActionsPatchControler {

        // rebind controls as layed out in the document
        public static bool Prefix(ref PlayerActions __result) {
            PlayerActions playerActions = new PlayerActions();
            playerActions.Fire.AddDefaultBinding(InputControlType.RightTrigger);
            playerActions.Block.AddDefaultBinding(InputControlType.LeftTrigger);
            playerActions.Jump.AddDefaultBinding(InputControlType.RightBumper);
            playerActions.Start.AddDefaultBinding(InputControlType.Start);
            playerActions.Left.AddDefaultBinding(InputControlType.LeftStickLeft);
            playerActions.Right.AddDefaultBinding(InputControlType.LeftStickRight);
            playerActions.Up.AddDefaultBinding(InputControlType.LeftStickUp);
            playerActions.Down.AddDefaultBinding(InputControlType.LeftStickDown);
            playerActions.AimLeft.AddDefaultBinding(InputControlType.RightStickLeft);
            playerActions.AimRight.AddDefaultBinding(InputControlType.RightStickRight);
            playerActions.AimUp.AddDefaultBinding(InputControlType.RightStickUp);
            playerActions.AimDown.AddDefaultBinding(InputControlType.RightStickDown);
            playerActions.InvokeMethod("CreatePlayerAction", "Grapple");
            __result =  playerActions;
            return false;
        }

    }
    [HarmonyPatch(typeof(PlayerActions), "CreateWithKeyboardBindings")]
    public class PlayerActionsPatchKeyboard {

        // rebind controls as layed out in the document
        public static void Postfix(ref PlayerActions __result) {
            ((PlayerAction)__result.InvokeMethod("CreatePlayerAction", "Grapple")).AddDefaultBinding(Key.E);
        }

    }
}
