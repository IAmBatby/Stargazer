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
        public RectTransform RectTransform { get; private set; }

        public VisualizerState CurrentState { get; private set; }

        public MoonGroupTargetLerpInfo CurrentLerpInfo { get; private set; }

        private Vector2 defaultOffsetMin;
        private Vector2 defaultOffsetMax;
        private Vector3 defaultLocalPosition;
        private List<MoonUIVisualizer> allMoonVisualizers = new List<MoonUIVisualizer>();
        private List<MoonLine> allMoonLines = new List<MoonLine>();

        private float defaultTime = 0.05f;
        private float activeTime = 0.05f;
        private float inactiveTime = 0.005f;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            defaultOffsetMin = RectTransform.offsetMin;
            defaultOffsetMax = RectTransform.offsetMax;
            defaultLocalPosition = RectTransform.localPosition;
            CurrentState = VisualizerState.Default;
            CurrentLerpInfo = GetTargetLerpInfo();
        }

        private void Update()
        {
            RectTransform.offsetMin = RectTransform.offsetMin.Lerp(CurrentLerpInfo.targetOffsetMin, CurrentLerpInfo.lerpTime);
            RectTransform.offsetMax = RectTransform.offsetMax.Lerp(CurrentLerpInfo.targetOffsetMax, CurrentLerpInfo.lerpTime);
            RectTransform.localPosition = RectTransform.localPosition.Lerp(CurrentLerpInfo.targetLocalPosition, CurrentLerpInfo.lerpTime);
            RectTransform.localScale = RectTransform.localScale.Lerp(CurrentLerpInfo.targetLocalScale, CurrentLerpInfo.lerpTime);
        }

        private MoonGroupTargetLerpInfo GetTargetLerpInfo()
        {
            MoonGroupTargetLerpInfo newInfo = CurrentLerpInfo;
            if (CurrentState == VisualizerState.Default)
                newInfo = new(defaultOffsetMin, defaultOffsetMax, defaultLocalPosition, Vector3.one, defaultTime);
            else if (CurrentState == VisualizerState.Active)
                newInfo = new(Vector2.zero, Vector2.zero, Vector3.zero, new Vector3(1.85f, 1.85f, 1f), activeTime);
            else if (CurrentState == VisualizerState.Inactive)
                newInfo = new(defaultOffsetMin, defaultOffsetMax, defaultLocalPosition, Vector3.zero, inactiveTime);
            return (newInfo);
        }

        public void SetVisualizerState(VisualizerState newState)
        {
            CurrentState = newState;
            CurrentLerpInfo = GetTargetLerpInfo();
        }

        public void Reset()
        {
            SetVisualizerState(VisualizerState.Default);

            RectTransform.offsetMin = CurrentLerpInfo.targetOffsetMin;
            RectTransform.offsetMax = CurrentLerpInfo.targetOffsetMax;
            RectTransform.localPosition = CurrentLerpInfo.targetLocalPosition;
            RectTransform.localScale = CurrentLerpInfo.targetLocalScale;
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
