using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer.Components
{
    public class MoonUIVisualizer : MonoBehaviour
    {
        public ExtendedLevel CurrentLevel { get; private set; }
        public VisualizerState CurrentState { get; private set; }
        public MoonTargetLerpInfo CurrentLerpInfo { get; private set; }

        public Color DefaultColor { get; private set; }
        public Color ActiveColor { get; private set; }

        [SerializeField] private float colorLerpValue = 0.1f;
        [SerializeField] private Image moonImage;
        [SerializeField] private TextMeshProUGUI moonTitle;
        [SerializeField] private List<Image> miscMoonImages = new List<Image>();
        [SerializeField] private RectTransform rectTransform;


        private void Awake()
        {
            if (CurrentLevel == null)
                enabled = false;
        }

        private void Update()
        {
            ApplyColor(Color.Lerp(ActiveColor, CurrentLerpInfo.targetColor, 1 - Mathf.Pow(CurrentLerpInfo.lerpTime, Time.deltaTime)));
        }

        private MoonTargetLerpInfo GetTargetLerpInfo()
        {
            MoonTargetLerpInfo newInfo = CurrentLerpInfo;
            if (CurrentState == VisualizerState.Default)
                newInfo = new(DefaultColor, colorLerpValue);
            else if (CurrentState == VisualizerState.Active)
                newInfo = new(DefaultColor, colorLerpValue);
            else if (CurrentState == VisualizerState.Inactive)
                newInfo = new(StarmapUIManager.InactiveColor, colorLerpValue);
            return (newInfo);
        }

        public void Initialize(ExtendedLevel newControllingLevel)
        {
            enabled = true;
            CurrentLevel = newControllingLevel;

            if (CurrentLevel.IsRouteHidden == true)
            {
                DefaultColor = Color.black;
                moonTitle.SetText("##. ?????");
            }
            else
            {
                foreach (ContentTag contentTag in CurrentLevel.ContentTags)
                    if (StarmapUIManager.starmapTags.TryGetValue(contentTag.TagName, out Color tagColor))
                    {
                        DefaultColor = tagColor;
                        break;
                    }

                moonTitle.SetText(CurrentLevel.SelectableLevel.PlanetName);
            }

            ApplyColor(DefaultColor);
            CurrentState = VisualizerState.Default;
            CurrentLerpInfo = GetTargetLerpInfo();
        }

        public void SetVisualizerState(VisualizerState newVisualizerState)
        {
            CurrentState = newVisualizerState;
            CurrentLerpInfo = GetTargetLerpInfo();
        }

        public void Reset()
        {
            SetVisualizerState(VisualizerState.Default);
            ApplyColor(CurrentLerpInfo.targetColor);
        }

        private void ApplyColor(Color color)
        {
            ActiveColor = color;
            moonImage.color = ActiveColor;
            moonTitle.color = ActiveColor;
            foreach (Image image in miscMoonImages)
                image.color = ActiveColor;
        }
    }

    public struct MoonTargetLerpInfo
    {
        public Color targetColor;
        public float lerpTime;

        public MoonTargetLerpInfo(Color newColor, float newLerpTime)
        {
            targetColor = newColor;
            lerpTime = newLerpTime;
        }
    }
}
