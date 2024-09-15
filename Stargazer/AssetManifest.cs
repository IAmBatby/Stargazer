using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    [CreateAssetMenu(fileName = "AssetManifest", menuName = "Stargazer/AssetManifest", order = 1)]
    public class AssetManifest : ScriptableObject
    {
        [Header("UI Assets")]
        public GameObject StarmapUIManagerPrefab;
        public GameObject MoonUIVisualizerPrefab;

        public GameObject MoonLine;

        public ColorManifest colorManifest;

        [Header("World Assets")]
        public GameObject StarmapManagerPrefab;

        public GameObject MoonGroupVisualizerPrefab;
        public GameObject MoonVisualizerPrefab;

        public Material BlackMaterial;
        public Material GreenMaterial;
    }
}
