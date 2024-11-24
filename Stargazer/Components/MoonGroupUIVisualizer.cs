using Stargazer.Components;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer
{
    public class MoonGroupUIVisualizer : MonoBehaviour
    {
        public RectTransform RectTransform { get; set; }

        public VisualizerState CurrentState { get; private set; }

        public MoonGroupTargetLerpInfo CurrentLerpInfo { get; private set; }

        [SerializeField] private Image debugBackground;

        private Vector2 defaultOffsetMin;
        private Vector2 defaultOffsetMax;
        private Vector3 defaultLocalPosition;
        public List<MoonUIVisualizer> allMoonVisualizers = new List<MoonUIVisualizer>();
        public List<MoonLine> allMoonLines = new List<MoonLine>();

        private Vector2 targetOffsetMin = new Vector2(0, 0);
        private Vector2 targetOffsetMax = new Vector2(-450, 300);
        //private Vector2 targetOffsetMin = new Vector2(0, 0);
        //private Vector2 targetOffsetMax = new Vector2(0, 0);

        private float defaultTime = 0.05f;
        private float activeTime = 0.05f;
        private float inactiveTime = 0.005f;

        private bool enableDebugging = false;
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            defaultOffsetMin = RectTransform.offsetMin;
            defaultOffsetMax = RectTransform.offsetMax;
            defaultLocalPosition = RectTransform.localPosition;
            CurrentState = VisualizerState.Default;
            CurrentLerpInfo = GetTargetLerpInfo();

            
            if (enableDebugging)
            {
                float randomR = UnityEngine.Random.Range(0.2f, 0.9f);
                float randomG = UnityEngine.Random.Range(0.2f, 0.9f);
                float randomB = UnityEngine.Random.Range(0.2f, 0.9f);
                debugBackground.color = new Color(randomR, randomG, randomB, 1f);
                debugBackground.enabled = true;
            }
            else
                debugBackground.enabled = false;

            
        }

        internal void ReorderChildren()
        {
            foreach (MoonLine moonLine in allMoonLines)
                moonLine.rectTransform.SetParent(RectTransform);
            foreach (MoonUIVisualizer visualizer in allMoonVisualizers)
                visualizer.rectTransform.SetParent(RectTransform);
        }

        private void Update()
        {
            //RectTransform.offsetMin = RectTransform.offsetMin.Lerp(CurrentLerpInfo.targetOffsetMin, CurrentLerpInfo.lerpTime);
            //RectTransform.offsetMax = RectTransform.offsetMax.Lerp(CurrentLerpInfo.targetOffsetMax, CurrentLerpInfo.lerpTime);
            //RectTransform.localPosition = RectTransform.localPosition.Lerp(CurrentLerpInfo.targetLocalPosition, CurrentLerpInfo.lerpTime);
            RectTransform.localScale = RectTransform.localScale.Lerp(CurrentLerpInfo.targetLocalScale, CurrentLerpInfo.lerpTime);
        }

        private MoonGroupTargetLerpInfo GetTargetLerpInfo()
        {
            MoonGroupTargetLerpInfo newInfo = CurrentLerpInfo;
            if (CurrentState == VisualizerState.Default)
                newInfo = new(defaultOffsetMin, defaultOffsetMax, defaultLocalPosition, new Vector3(0.9f,0.9f,0.9f), defaultTime);
            else if (CurrentState == VisualizerState.Active)
                newInfo = new(targetOffsetMin, targetOffsetMax, Vector3.zero, new Vector3(1.7f, 1.7f, 1f), activeTime);
            else if (CurrentState == VisualizerState.Inactive)
                newInfo = new(defaultOffsetMin, defaultOffsetMax, defaultLocalPosition, new Vector3(0.9f, 0.9f, 0.9f), inactiveTime);
            return (newInfo);
        }

        public void SetVisualizerState(VisualizerState newState, bool applyOnSet = false)
        {
            CurrentState = newState;
            CurrentLerpInfo = GetTargetLerpInfo();

            foreach (MoonLine moonLine in allMoonLines)
                moonLine.RefreshLine();

            if (applyOnSet == true)
            {
                RectTransform.offsetMin = CurrentLerpInfo.targetOffsetMin;
                RectTransform.offsetMax = CurrentLerpInfo.targetOffsetMax;
                RectTransform.localPosition = CurrentLerpInfo.targetLocalPosition;
                RectTransform.localScale = CurrentLerpInfo.targetLocalScale;
            }
        }
    }

    public struct MoonGroupTargetLerpInfo
    {
        public Vector2 targetOffsetMin;
        public Vector2 targetOffsetMax;
        public Vector3 targetLocalPosition;
        public Vector3 targetLocalScale;
        public float lerpTime;

        public MoonGroupTargetLerpInfo(Vector2 newOffsetMin, Vector2 newOffsetMax, Vector2 newLocalPosition, Vector3 newLocalScale, float newLerpTime)
        {
            targetOffsetMin = newOffsetMin;
            targetOffsetMax = newOffsetMax;
            targetLocalPosition = newLocalPosition;
            targetLocalScale = newLocalScale;
            lerpTime = newLerpTime;
        }
    }
}
