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
        public MoonGroupUIVisualizer CurrentMoonGroup { get; private set; }
        public VisualizerState CurrentState { get; private set; }
        public MoonTargetLerpInfo CurrentLerpInfo { get; private set; }

        public Color DefaultColor { get; private set; }
        public Color ActiveColor { get; private set; }

        [SerializeField] private float colorLerpValue = 0.025f;
        [SerializeField] internal Image moonImage;
        [SerializeField] private TextMeshProUGUI moonTitle;
        [SerializeField] private List<Image> miscMoonImages = new List<Image>();
        [SerializeField] internal RectTransform rectTransform;
        [SerializeField] internal Image weatherImage;

        private void Awake()
        {
            if (CurrentLevel == null)
                enabled = false;
        }

        private void Update()
        {
            ApplyColor(Color.Lerp(ActiveColor, CurrentLerpInfo.targetColor, 1 - Mathf.Pow(CurrentLerpInfo.lerpTime, Time.deltaTime)));
        }

        public void Initialize(ExtendedLevel newControllingLevel, MoonGroupUIVisualizer newMoonGroup)
        {
            enabled = true;
            CurrentLevel = newControllingLevel;
            CurrentMoonGroup = newMoonGroup;

            RefreshVisualInfo();

            SetVisualizerState(VisualizerState.Default, true);
        }

        public void RefreshVisualInfo()
        {
            DefaultColor = Color.white;
            if (!CurrentLevel.NumberlessPlanetName.Contains("Gordion") && CurrentLevel.IsRouteHidden == true)
            {
                DefaultColor = Color.gray;
                moonTitle.SetText("##. ?????");
            }
            else
            {
                foreach (ContentTag contentTag in CurrentLevel.ContentTags)
                    if (StarmapUIManager.StarmapTags.TryGetValue(contentTag.TagName, out Color tagColor))
                    {
                        DefaultColor = tagColor;
                        break;
                    }

                moonTitle.SetText(CurrentLevel.SelectableLevel.PlanetName);
            }
            RefreshWeatherIcon();

        }

        public void RefreshWeatherIcon()
        {
            Assets.Manifest.SetWeatherSprite(weatherImage, CurrentLevel.SelectableLevel.currentWeather);
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

        public void SetVisualizerState(VisualizerState newVisualizerState, bool applyOnSet = false)
        {
            CurrentState = newVisualizerState;
            CurrentLerpInfo = GetTargetLerpInfo();

            if (applyOnSet == true)
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
