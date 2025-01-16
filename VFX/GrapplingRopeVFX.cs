using System;
using System.Collections.Generic;
using System.Linq;

using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace RCO.VFX
{
    public class GrapplingRopeVFX : MonoBehaviour
    {
        public float vertexPerUnit = 5.0f;

        // attach this component to the source object (Gun) then cast the line to target object (Player / hook point)
        public GameObject targetObject = null;

        // rope displacement from center line; max based current rope length
        public float ropeAmpScaling = 0.1f;

        // rope wave counts per unit
        public float ropeWaveRate = 1 / 5.0f;

        // rope wave 'speed'
        public float ropeSwingRate = 25f;

        // rope snap time when latched
        public float ropeSnapTime = 0.15f;
        public float ropeOvershotTime = 0.1f;
        public float ropeOvershotFactor = 1.25f;

        public float ropeSpeed = 150.0f;

        // animation vars
        private int ropeVertexCount = 0;
        private List<Vector3> ropeVertexList;

        private float ropeCurrentAmpMult = 1.0f;
        private float ropeCurrentAmpMax;

        private float ropeSwingTimer = 0.0f;
        private float ropeLatchTimer = 0.0f;

        private bool isRopeLatched = false;
        private bool isRopeRetracting = false;
        private bool isAnimationDone = false;

        private LineRenderer lineRenderer;
        public Vector3 targetLastPos;
        private Vector3 ropeEndPos;

        private Vector3 ropeLine;
        private Vector3 ropeWaveVector;

        // debug vars
        // public bool resetState = false;
        // public bool nextState = false;


        void Start()
        {
            lineRenderer = gameObject.GetComponent<LineRenderer>();

            if (targetObject == null)
            {
                // targetLastPos = Vector3.zero;
                // UnityEngine.Debug.LogWarning("Rope not attached!");
            }
            else
            {
                targetLastPos = targetObject.transform.position;
            }

            ropeEndPos = transform.position;
        }

        void Update()
        {
            if (isAnimationDone) return;

            // update base variables
            if (targetObject != null)
            {
                targetLastPos = targetObject.transform.position;
            }

            ropeLine = (ropeEndPos - transform.position);
            ropeLine.z = 0.0f;
            ropeWaveVector = RotateVector(ropeLine, 90.0f).normalized;

            ropeCurrentAmpMax = ropeAmpScaling * ropeLine.magnitude;

            ropeVertexCount = Mathf.CeilToInt(ropeLine.magnitude * vertexPerUnit);

            // update LineRenderer pos
            ropeVertexList = new List<Vector3>();

            for (int i = 0; i <= ropeVertexCount; i++)
            {
                float iPos = (float)i / (float)ropeVertexCount;

                float pointAmp = Mathf.Sin(Mathf.PI * iPos) * ropeCurrentAmpMax * ropeCurrentAmpMult;

                float waveCount = ropeLine.magnitude * ropeWaveRate;

                float waveTime = ropeSwingTimer * ropeSwingRate;

                float pointDisplace = Mathf.Sin((Mathf.PI * waveCount * iPos) - waveTime) * pointAmp;

                Vector3 vertex = transform.position + (ropeLine * i / ropeVertexCount) + (ropeWaveVector * pointDisplace);

                ropeVertexList.Add(vertex);
            }

            lineRenderer.positionCount = ropeVertexList.Count;
            lineRenderer.SetPositions(ropeVertexList.ToArray());

            // state and timer

            if (!isRopeLatched)
            {
                // animate the rope grappling the target
                float displacement = ropeSpeed * TimeHandler.deltaTime;

                if ((targetLastPos - ropeEndPos).magnitude < displacement)
                {
                    ropeEndPos = targetLastPos;
                    isRopeLatched = true;
                }
                else
                {
                    ropeEndPos += (targetLastPos - ropeEndPos).normalized * displacement;
                }
            }

            if (!isRopeRetracting)
            {
                // animate the rope wave
                ropeSwingTimer += TimeHandler.deltaTime;
                ropeCurrentAmpMult = 1.0f;
            }
            else
            {
                // animate the rope retracting
                float retractDistance = ropeSpeed * TimeHandler.deltaTime;

                if ((transform.position - ropeEndPos).magnitude < retractDistance)
                {
                    isAnimationDone = true;
                    lineRenderer.enabled = false;
                    Destroy(this.gameObject);
                    return;
                }
                else
                {
                    ropeEndPos += (transform.position - ropeEndPos).normalized * retractDistance;
                }

            }

            if (isRopeLatched)
            {
                // animate the rope latching
                ropeLatchTimer = Mathf.Clamp(ropeLatchTimer + TimeHandler.deltaTime, 0.0f, ropeSnapTime + ropeOvershotTime);

                if (ropeLatchTimer < ropeSnapTime)
                {
                    ropeCurrentAmpMult = (ropeSnapTime - ropeLatchTimer) / ropeSnapTime;
                }
                else
                {
                    float overshootTimer = ropeLatchTimer - ropeSnapTime;
                    ropeCurrentAmpMult = -Mathf.Sin(Mathf.PI * (overshootTimer / ropeOvershotTime)) * ropeCurrentAmpMax * ropeOvershotFactor;
                }

                if (ropeLatchTimer + TimeHandler.deltaTime > ropeOvershotTime)
                {
                    isRopeRetracting = true;
                }
            }

        }

        // Vector Utils
        private static Vector3 RotateVector(Vector3 vector, float degree)
        {
            float sin = Mathf.Sin(degree * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degree * Mathf.Deg2Rad);

            float prevX = vector.x;
            float prevY = vector.y;

            vector.x = (cos * prevX) - (sin * prevY);
            vector.y = (sin * prevX) + (cos * prevY);

            return vector;
        }

    }
}