using System;
using System.Collections.Generic;
using System.Linq;

using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace RCO.VFX
{
    public class SimpleStatusVFX : MonoBehaviour
    {
        public static float FadeTime = 0.05f;
        public static float FadeMinScale = 0.01f;

        // animation vars
        private float animTimer = 0.0f;
        public bool visible = false;
        public GameObject targetObject = null;
        public Vector3 originalScale = Vector3.one;
        
        void Start()
        {
            if (targetObject != null)
            {
                originalScale = targetObject.transform.localScale;
            }
        }

        void Update()
        {
            if (targetObject != null)
            {
                if (visible)
                {
                    animTimer += TimeHandler.deltaTime;
                    if (animTimer > FadeTime) { animTimer = FadeTime; }
                    if (animTimer > 0.0f) { targetObject.SetActive(true); }
                }
                else
                {
                    animTimer -= TimeHandler.deltaTime;
                    if (animTimer < 0.0f)
                    { 
                        animTimer = 0.0f;
                        targetObject.transform.localScale = Vector3.one * FadeMinScale;
                        targetObject.SetActive(false);
                        return;
                    }

                }

                targetObject.transform.localScale = originalScale * animTimer / FadeTime;
            }
        }
    }
}