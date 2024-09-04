using HarmonyLib;
using InControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RCO.Patches {
    [HarmonyPatch(typeof(PlayerActions),"CreateWithControllerBindings")]
    public class PlayerActionsPatch {

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
            __result =  playerActions;
            return false;
        }

    }
}
