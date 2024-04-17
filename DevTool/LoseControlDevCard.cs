using System;
using System.Collections.Generic;
using System.Linq;

using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

using RCO.MonoBehaviours;
using RCO.Extensions;

// dev card, DO NOT INCLUDE IN FINAL BUILD!
namespace RCO.Dev
{
    class LoseControlDevCard : CustomCard
    {
        public static GameObject objectToSpawn = null;

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            // gun.attackSpeed = 1.0f / 0.85f;
        }

        // "attackSpeed" is technically a gunfire cooldown between shots >> Less is more rapid firing
        // 'attackSpeedMultiplier' works as intended >> More is more rapid firing
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            if (objectToSpawn == null)
            {
                objectToSpawn = new GameObject("LoseControlBullet", new Type[]
                {
                        typeof(LoseControlBullet)
                });
                DontDestroyOnLoad(objectToSpawn);
            }

            List<ObjectsToSpawn> list = gun.objectsToSpawn.ToList<ObjectsToSpawn>();
            list.Add(new ObjectsToSpawn
            {
                AddToProjectile = objectToSpawn
            });

            gun.objectsToSpawn = list.ToArray();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // bullet modifier pool auto-reset on card removal, simply let it do its jobs
            // UnityEngine.Debug.Log($"[{GearUpCards.ModInitials}][Card] {GetTitle()} has been removed to player {player.playerID}.");
        }
        protected override string GetTitle()
        {
            return "DEV Lose Control";
        }
        protected override string GetDescription()
        {
            return "shoot player to apply [Lose Control] status effect";
        }
        protected override GameObject GetCardArt()
        {
            return null;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Common;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
               
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.TechWhite;
        }
        public override string GetModName()
        {
            return "DEV";
        }

        public class LoseControlBullet : RayHitEffect
        {
            public static float debuffDuration = 2f;

            public override HasToReturn DoHitEffect(HitInfo hit)
            {
                if (hit.transform == null)
                {
                    return HasToReturn.canContinue;
                }
                if (hit.transform.gameObject.tag.Contains("Bullet"))
                {
                    return HasToReturn.canContinue;
                }

                if (hit.transform.GetComponent<Player>())
                {
                    CharacterData victim = hit.transform.gameObject.GetComponent<CharacterData>();

                    LoseControlHandler handler = victim.player.gameObject.GetOrAddComponent<LoseControlHandler>();

                    handler.RPCA_AddLoseControl(debuffDuration);
                }

                return HasToReturn.canContinue;
            }

            public void Destroy()
            {
                // UnityEngine.Object.Destroy(this);
            }

        }
    }
}
