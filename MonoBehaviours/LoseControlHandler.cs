using Photon.Pun;
using RCO.Extensions;
using UnityEngine;

namespace RCO.MonoBehaviours
{
    public class LoseControlHandler : MonoBehaviour
    {
        // [Header("Settings")]
        // public CodeAnimation codeAnim;

        private Player player;
        private CharacterData data;
        private CharacterStatModifiers stats;

        private void Start()
        {
            player = GetComponent<Player>();
            data = player.data;
        }

        private void Update()
        {
            if (data.GetOverhaulData().loseControlTimer > 0f)
            {
                data.GetOverhaulData().loseControlTimer -= TimeHandler.deltaTime;
                if (!data.GetOverhaulData().isLostControl)
                {
                    StartLoseControl();
                }
            }
            else if (data.GetOverhaulData().isLostControl)
            {
                StopLoseControl();
            }
        }

        private void StartLoseControl()
        {
            // player.data.input.silencedInput = true;
            // codeAnim.PlayIn();
            data.GetOverhaulData().isLostControl = true;
        }

        public void StopLoseControl()
        {
            //player.data.input.silencedInput = false;

            // if (codeAnim.currentState == CodeAnimationInstance.AnimationUse.In)
            // {
            //     codeAnim.PlayOut();
            // }

            data.GetOverhaulData().isLostControl = false;
            data.GetOverhaulData().loseControlTimer = 0f;
        }

        private void OnDisable()
        {
            // codeAnim.transform.localScale = Vector3.zero;

            // only ever run when reviving...?
            StopLoseControl();
        }

        [PunRPC]
        public void RPCA_AddLoseControl(float time)
        {
            if (time > data.GetOverhaulData().loseControlTimer)
            {
                data.GetOverhaulData().loseControlTimer = time;
            }

            if (!data.GetOverhaulData().isLostControl)
            {
                StartLoseControl();
            }
        }
    }
}
