using LethalLevelLoader;
using Stargazer.Components;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Stargazer
{
    public class MoonDataVisualiser : MonoBehaviour
    {
        public RectTransform rectTransform;

        public TextMeshProUGUI titleText;

        public TextMeshProUGUI descriptionText;

        private float lerpTime = 0.05f;

        private void Update()
        {
            MoonUIVisualizer activeMoon = StarmapUIManager.Instance.ActiveMoon;
            if (activeMoon != null)
            {
                titleText.SetText(activeMoon.CurrentLevel.SelectableLevel.PlanetName);
                descriptionText.SetText(activeMoon.CurrentLevel.SelectableLevel.LevelDescription);

                rectTransform.localScale = rectTransform.localScale.Lerp(Vector3.one, lerpTime);
            }
            else
                rectTransform.localScale = rectTransform.localScale.Lerp(Vector3.zero, lerpTime);
        }
    }
}
