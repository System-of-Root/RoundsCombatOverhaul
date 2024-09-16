using RCO.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace RCO.MonoBehaviours
{
    class LoseControlMonitor : MonoBehaviour
    {
        public List<GameObject> watchList = new List<GameObject>();

        void Update()
        {
            if (watchList.Count > 0)
            {
                CharacterData characterData;
                foreach (GameObject player in watchList)
                {
                    characterData = player.GetComponent<CharacterData>();
                    characterData.GetOverhaulData().isLostControl = false;
                    characterData.GetOverhaulData().loseControlTimer = 0.0f;

                    characterData.GetOverhaulData().isDisarmed = false;
                    characterData.GetOverhaulData().disarmedTimer = 0.0f;

                    characterData.GetOverhaulData().isImmobile = false;
                    characterData.GetOverhaulData().immobileTimer = 0.0f;

                    // UnityEngine.Debug.Log($"Player[{characterData.player.playerID}] died and reset [Lose Control]");
                    // UnityEngine.Debug.Log($"Player[{characterData.player.playerID}] {characterData.GetOverhaulData().isLostControl}");
                    // UnityEngine.Debug.Log($"Player[{characterData.player.playerID}] {characterData.GetOverhaulData().loseControlTimer}");
                }

                watchList.Clear();
            }
        }
    }
}
