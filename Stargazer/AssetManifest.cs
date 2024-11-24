using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer
{
    [CreateAssetMenu(fileName = "AssetManifest", menuName = "Stargazer/AssetManifest", order = 1)]
    public class AssetManifest : ScriptableObject
    {
        [Header("UI Assets")]
        public GameObject StarmapUIManagerPrefab;
        public GameObject MoonGroupUIVisualizerPrefab;
        public GameObject MoonUIVisualizerPrefab;

        public GameObject MoonLine;

        public ColorManifest colorManifest;

        public Sprite foggySprite;
        public Sprite rainySprite;
        public Sprite stormySprite;
        public Sprite eclipseSprite;

        [Header("World Assets")]
        public GameObject StarmapManagerPrefab;

        public GameObject MoonGroupVisualizerPrefab;
        public GameObject MoonVisualizerPrefab;

        public Material BlackMaterial;
        public Material GreenMaterial;

        public void SetWeatherSprite(Image image, LevelWeatherType type)
        {
            image.sprite = type switch
            {
                LevelWeatherType.Rainy => Assets.Manifest.rainySprite,
                LevelWeatherType.Stormy => Assets.Manifest.stormySprite,
                LevelWeatherType.Foggy => Assets.Manifest.foggySprite,
                LevelWeatherType.Eclipsed => Assets.Manifest.eclipseSprite,
                _ => null
            };
            if (image.sprite == null)
                image.gameObject.SetActive(false);
            else
                image.gameObject.SetActive(true);
        }
    }
}
