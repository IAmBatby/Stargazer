using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    public enum MoonBackgroundSetting { Transparent, Skybox }
    public class VisualMoonCatalogueContainer : MonoBehaviour
    {
        public GameObject plane;
        public Camera camera;
        public RenderTexture renderTexture;
        public Material terminalGreen;
        public Material terminalBlack;

        public GameObject visualMoonGroupContainerPrefab;
        public GameObject visualMoonContainerPrefab;

        public MoonBackgroundSetting backgroundSetting;
        public bool useTagColors = true;

        public List<VisualMoonGroupContainer> visualMoonGroupContainersList = new List<VisualMoonGroupContainer>();
        public List<VisualMoonContainer> visualMoonContainersList = new List<VisualMoonContainer>();


        public Vector3 visualMoonCatalogueSpawnPosition = new Vector3(0, 150, 0);
        public float extendedLevelGroupMultiplier = 25f;
        public float extendedLevelMultiplier = 7.5f;
        public float lineRendererLineWidth = 3f;
        public float extendedLevelOffset = 45;

        public int textFontSize = 105;
        public float textOffsetX = 12;
        public float textOffsetZ = -7;

        public void Update()
        {
            if (Patches.terminal.currentNode == Patches.moonsCatalogueNode)
            {
                    
            }
        }

        public void UpdateVisualMoons()
        {
            foreach (VisualMoonContainer moonContainer in visualMoonContainersList)
            {
                moonContainer.spawnedMoonText.transform.position = moonContainer.spawnedMoonObject.transform.position;
                moonContainer.spawnedMoonText.transform.position += new Vector3(textOffsetX, 0, textOffsetZ);
                moonContainer.spawnedMoonText.fontSize = textFontSize;
            }
        }
    }
}
