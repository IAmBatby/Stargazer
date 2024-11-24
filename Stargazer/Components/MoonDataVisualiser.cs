using LethalLevelLoader;
using Stargazer.Components;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer
{
    public class MoonDataVisualiser : MonoBehaviour
    {
        public RectTransform rectTransform;

        public TextMeshProUGUI titleText;

        public TextMeshProUGUI descriptionText;

        private float lerpTime = 0.05f;

        MoonUIVisualizer previousMoon;

        public Image weatherImage;

        private void Update()
        {
            MoonUIVisualizer activeMoon = StarmapUIManager.Instance.ActiveMoon;

            if (activeMoon != null)
            {
                rectTransform.localScale = rectTransform.localScale.Lerp(new Vector3(0.85f,0.85f,0.85f), lerpTime);
                if (activeMoon != previousMoon)
                {
                    Assets.Manifest.SetWeatherSprite(weatherImage, activeMoon.CurrentLevel.SelectableLevel.currentWeather);
                    UpdateInfo(activeMoon.CurrentLevel.SelectableLevel.PlanetName, GetGeneralMoonInfo(activeMoon.CurrentLevel));
                }
            }
            else
                rectTransform.localScale = rectTransform.localScale.Lerp(Vector3.zero, lerpTime);

            previousMoon = activeMoon;
        }

        public void UpdateInfo(string newTitleText = null, string newDescriptionText = null)
        {
            if (!string.IsNullOrEmpty(newTitleText))
                titleText.SetText(newTitleText);
            if (!string.IsNullOrEmpty(newDescriptionText))
                descriptionText.SetText(newDescriptionText);
        }

        public string GetGeneralMoonInfo(ExtendedLevel level)
        {
            string returnString = string.Empty;

            returnString += "Route Price: $" + level.RoutePrice + "\n";
            returnString += "Current Weather: " + level.SelectableLevel.currentWeather.ToString() + "\n";
            returnString += "Evaluated Difficulty: " + level.SelectableLevel.riskLevel + "\n";
            returnString += "Environmental Database Attributes: ";
            foreach (ContentTag contentTag in level.ContentTags)
                if (FilterArbitraryTags(level, contentTag))
                    returnString += "\"" + contentTag.TagName + "\"" + ", ";
            if (returnString.Contains(", "))
                returnString = returnString.Remove(returnString.LastIndexOf(", "), 2);
            else
                returnString += "UNKNOWN";

            if (LevelManager.CurrentExtendedLevel == level)
                returnString += "\n\n" + "- Currently orbiting this moon.";
            return (returnString);
        }

        private bool FilterArbitraryTags(ExtendedLevel level, ContentTag contentTag)
        {
            List<string> filters = new List<string>()
            {
                "vanilla", "custom", "free", "paid"
            };

            if (filters.Contains(contentTag.TagName.ToLower()))
                return (false);
            if (contentTag.TagName.StripSpecialCharacters().ToLower().Contains(level.NumberlessPlanetName.StripSpecialCharacters().ToLower()))
                return (false);
            if (level.ExtendedMod.AuthorName.Contains(contentTag.TagName))
                return (false);
            return (true);
        }
    }
}
