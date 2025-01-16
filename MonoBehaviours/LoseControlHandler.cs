using Photon.Pun;
using RCO.Extensions;
using RCO.VFX;
using UnboundLib;
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

        private StunHandler stunHandler;

        public SimpleStatusVFX LoseControlVFX = null;
        public SimpleStatusVFX DisarmedVFX = null;

        private void Start()
        {
            player = GetComponent<Player>();
            data = player.data;

            stunHandler = GetComponent<StunHandler>();
        }

        private void Update()
        {
            // [Lose Control] section
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

            // [Disarmed] section
            if (data.GetOverhaulData().disarmedTimer > 0f)
            {
                data.GetOverhaulData().disarmedTimer -= TimeHandler.deltaTime;
                if (!data.GetOverhaulData().isDisarmed)
                {
                    StartDisarmed();
                }
            }
            else if (data.GetOverhaulData().isDisarmed)
            {
                StopDisarmed();
            }

            // [Immobile] section
            if (data.GetOverhaulData().immobileTimer > 0f)
            {
                data.GetOverhaulData().immobileTimer -= TimeHandler.deltaTime;
                if (!data.GetOverhaulData().isImmobile)
                {
                    StartImmobile();
                }
            }
            else if (data.GetOverhaulData().isImmobile)
            {
                StopImmobile();
            }
        }

        // status effect methods
        private void StartLoseControl()
        {
            LoseControlVFX.visible = true;

            data.GetOverhaulData().isLostControl = true;
        }
        public void StopLoseControl()
        {
            LoseControlVFX.visible = false;

            data.GetOverhaulData().isLostControl = false;
            data.GetOverhaulData().loseControlTimer = 0f;
        }

        private void StartDisarmed()
        {
            DisarmedVFX.visible = true;

            data.GetOverhaulData().isDisarmed = true;
        }
        public void StopDisarmed()
        {
            DisarmedVFX.visible = false;

            data.GetOverhaulData().isDisarmed = false;
            data.GetOverhaulData().disarmedTimer = 0f;
        }

        private void StartImmobile()
        {
            // - placeholder for animation starter - 

            data.GetOverhaulData().isImmobile = true;
        }
        public void StopImmobile()
        {
            // - placeholder for animation stopper - 

            data.GetOverhaulData().isImmobile = false;
            data.GetOverhaulData().immobileTimer = 0f;
        }

        private void OnDisable()
        {
            // stop all animations

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

        [PunRPC]
        public void RPCA_AddDisarmed(float time)
        {
            if (time > data.GetOverhaulData().disarmedTimer)
            {
                data.GetOverhaulData().disarmedTimer = time;
            }

            if (!data.GetOverhaulData().isDisarmed)
            {
                StartDisarmed();
            }
        }

        [PunRPC]
        public void RPCA_AddImmobile(float time)
        {
            if (time > data.GetOverhaulData().immobileTimer)
            {
                data.GetOverhaulData().immobileTimer = time;
            }

            if (!data.GetOverhaulData().isImmobile)
            {
                StartImmobile();
            }
        }

        [PunRPC]
        public void RPCA_SetImmobile(float time)
        {
            data.GetOverhaulData().immobileTimer = time;

            if (!data.GetOverhaulData().isImmobile && time > 0.0f)
            {
                StartImmobile();
            }
            else if (time <= 0.0f)
            {
                data.GetOverhaulData().immobileTimer = 0.0f;
                StopImmobile();
            }
        }

        [PunRPC]
        public void RPCA_SetDisarm(float time)
        {
            data.GetOverhaulData().disarmedTimer = time;

            if (!data.GetOverhaulData().isDisarmed && time > 0.0f)
            {
                StartDisarmed();
            }
            else if (time <= 0.0f)
            {
                data.GetOverhaulData().disarmedTimer = 0.0f;
                StopDisarmed();
            }
        }

        [PunRPC]
        public void RPCA_SetStun(float time)
        {
            data.stunTime = time;

            if (!data.isStunned && time > 0.0f)
            {
                stunHandler.InvokeMethod("StartStun");
            }
            else if (time <= 0.0f)
            {
                data.stunTime = 0.0f;
                stunHandler.StopStun();
            }
        }
    }
}
