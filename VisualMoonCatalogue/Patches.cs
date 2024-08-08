using HarmonyLib;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.TerrainUtils;
using static Unity.Audio.Handle;

namespace VisualMoonCatalogue
{
    internal static class Patches
    {
        internal static bool hasStarted = false;
        internal static GameObject visualMoonCatalogueContainerPrefab;
        internal static VisualMoonCatalogueContainer visualMoonCatalogue;
        internal static TerminalNode moonsCatalogueNode;
        internal static Terminal terminal;
        internal static Material sampleSceneRelaySkybox;
        

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPostfix]
        internal static void RoundManagerUpdate_Prefix()
        {
            Plugin.DebugLog("Starting VisualMoonCatalogue!");

            if (TerminalManager.currentMoonsCataloguePage.ExtendedLevels.Count == 0)
                Plugin.DebugLog("No Levels Found!");

            if (hasStarted == true) return;

            sampleSceneRelaySkybox = RenderSettings.skybox;

            InstansiateVisualMoonCatalogueContainer();

            AddRenderTextureToMoonsCatalogueNode();
            hasStarted = true;
        }


        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        [HarmonyPostfix]
        internal static void TerminalTextPostProcess_Postfix(TerminalNode node, ref string __result)
        {
            if (node == moonsCatalogueNode)
            {
                Plugin.DebugLog("Requesting Copy Texture Refresh!");
                RenderPipelineManager.endCameraRendering += RefreshCopyTexture;
                __result = string.Empty;
            }

        }

        internal static void RefreshCopyTexture(ScriptableRenderContext context, Camera camera)
        {
            if (camera == visualMoonCatalogue.camera)
            {
                if (visualMoonCatalogue.backgroundSetting == MoonBackgroundSetting.Transparent)
                {
                    Plugin.DebugLog("Applying New Copy Of RenderTexture!");
                    RenderTexture renderTexture = visualMoonCatalogue.renderTexture;
                    RenderTexture tempRenderTexture = RenderTexture.active;
                    RenderTexture.active = renderTexture;
                    Texture2D newTexture = new Texture2D(renderTexture.width, renderTexture.height);
                    newTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    newTexture.Apply();

                    Color[] pixels = newTexture.GetPixels();

                    int foundPixels = 0;
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        float combinedValue = (pixels[i].r * 255) + (pixels[i].g * 255) + (pixels[i].b * 255);
                        if (combinedValue < 50)
                        {
                            foundPixels++;
                            pixels[i] = Color.clear;
                        }
                        //pixels[i] = Color.red;
                    }

                    Plugin.DebugLog("Removed: " + foundPixels + " Black Pixels.");

                    newTexture.SetPixels(pixels);
                    newTexture.Apply();

                    moonsCatalogueNode.displayTexture = newTexture;
                    RenderTexture.active = tempRenderTexture;
                    RenderPipelineManager.endCameraRendering -= RefreshCopyTexture;
                    Plugin.DebugLog("Finished Applying New Copy Of RenderTexture!");
                }
            }
        }

        static public Texture2D ToTexture2D(RenderTexture rTex)
        {
            Texture2D dest = new Texture2D(rTex.width, rTex.height, GraphicsFormatUtility.GetTextureFormat(GraphicsFormatUtility.GetGraphicsFormat(rTex.format, false)), false);
            dest.Apply(false);
            Graphics.CopyTexture(rTex, dest);
            return dest;
        }

        internal static void InstansiateVisualMoonCatalogueContainer()
        {
            GameObject visualMoonCatalogueContainerObject = GameObject.Instantiate(visualMoonCatalogueContainerPrefab);
            visualMoonCatalogue = visualMoonCatalogueContainerObject.GetComponent<VisualMoonCatalogueContainer>();
            visualMoonCatalogueContainerObject.transform.position = visualMoonCatalogue.visualMoonCatalogueSpawnPosition;

            visualMoonCatalogue.backgroundSetting = MoonBackgroundSetting.Transparent;

            if (visualMoonCatalogue.backgroundSetting == MoonBackgroundSetting.Transparent)
            {
                visualMoonCatalogue.plane.GetComponent<MeshRenderer>().enabled = false;
                visualMoonCatalogue.useTagColors = false;
            }
            else
            {
                visualMoonCatalogue.useTagColors = true;
                visualMoonCatalogue.plane.transform.position = visualMoonCatalogue.visualMoonCatalogueSpawnPosition;
                visualMoonCatalogue.plane.transform.localScale = new Vector3(500, 1, 500);

                GameObject backingPlane = GameObject.Instantiate(visualMoonCatalogue.plane, visualMoonCatalogue.transform);
                backingPlane.GetComponent<MeshRenderer>().SetMaterial(visualMoonCatalogue.terminalBlack);
                backingPlane.transform.position -= new Vector3(0, 5, 0);
            }

            visualMoonCatalogue.camera.clearFlags = CameraClearFlags.Nothing;
            visualMoonCatalogue.camera.farClipPlane = 550;

            List<ExtendedLevelGroup> extendedLevelGroups = new List<ExtendedLevelGroup>(TerminalManager.defaultMoonsCataloguePage.ExtendedLevelGroups);

            ExtendedLevelGroup gordionGroup = new ExtendedLevelGroup(new List<ExtendedLevel>(){PatchedContent.ExtendedLevels[3]});

            if (extendedLevelGroups.Count > 4)
                GalaxyOffsets.currentSolarSystemSize = SolarSystemSize.Eight;
            else if (extendedLevelGroups.Count > 3)
                GalaxyOffsets.currentSolarSystemSize = SolarSystemSize.Four;
            else
                GalaxyOffsets.currentSolarSystemSize = SolarSystemSize.Three;

            Plugin.DebugLog("Current SolarSystemSize Set To: " + GalaxyOffsets.currentSolarSystemSize);

            VisualMoonGroupContainer gordionMoonGroupContainer = InstansiateVisualMoonGroupContainer(gordionGroup, Vector2.zero);

            foreach (ExtendedLevelGroup extendedLevelGroup in extendedLevelGroups)
                visualMoonCatalogue.visualMoonGroupContainersList.Add(InstansiateVisualMoonGroupContainer(extendedLevelGroup, GalaxyOffsets.GetGalaxyOffset(visualMoonCatalogue.visualMoonGroupContainersList.Count)));

            List<Vector3> gordionMoonGroupLineRendererPositions = new List<Vector3>();
            gordionMoonGroupLineRendererPositions.Add(gordionMoonGroupContainer.lineRenderer.GetPosition(0));
            foreach (VisualMoonGroupContainer visualMoonGroup in visualMoonCatalogue.visualMoonGroupContainersList)
            {
                Vector3 groupOffset = new Vector3((visualMoonCatalogue.extendedLevelOffset * visualMoonGroup.visualMoonContainersList.Count) / 2, 0, 0); ;
                visualMoonGroup.transform.position -= groupOffset;
                for (int i = 0; i < visualMoonGroup.visualMoonContainersList.Count; i++)
                    visualMoonGroup.lineRenderer.SetPosition(i, visualMoonGroup.lineRenderer.GetPosition(i) - groupOffset);

                VisualMoonContainer closestVisualMoon = null;
                foreach (VisualMoonContainer visualMoon in visualMoonGroup.visualMoonContainersList)
                {
                    if (closestVisualMoon == null)
                        closestVisualMoon = visualMoon;
                    else if (Vector3.Distance(gordionMoonGroupContainer.visualMoonContainersList[0].spawnedMoonObject.transform.position, closestVisualMoon.spawnedMoonObject.transform.position) > Vector3.Distance(gordionMoonGroupContainer.visualMoonContainersList[0].spawnedMoonObject.transform.position, visualMoon.spawnedMoonObject.transform.position))
                        closestVisualMoon = visualMoon;
                }
                gordionMoonGroupLineRendererPositions.Add(closestVisualMoon.spawnedMoonObject.transform.position);
            }
            List<Vector3> gordionMoonGroupLineRendererOffsetPositions = new List<Vector3>();
            foreach (Vector3 position in gordionMoonGroupLineRendererPositions)
                gordionMoonGroupLineRendererOffsetPositions.Add(new Vector3(position.x, visualMoonCatalogue.visualMoonCatalogueSpawnPosition.y + 10, position.z));
            gordionMoonGroupContainer.lineRenderer.positionCount = gordionMoonGroupLineRendererOffsetPositions.Count;
            gordionMoonGroupContainer.lineRenderer.SetPositions(gordionMoonGroupLineRendererOffsetPositions.ToArray());
            visualMoonCatalogue.camera.transform.position = new Vector3(gordionMoonGroupContainer.transform.position.x, visualMoonCatalogue.visualMoonCatalogueSpawnPosition.y + 200, gordionMoonGroupContainer.transform.position.z);
            visualMoonCatalogue.camera.orthographicSize = 190 + (7 * visualMoonCatalogue.visualMoonGroupContainersList.Count);
            string debugString = "Finished Instansiating Visual Moon Catalogue \n \n";
            int counter = 0;
            foreach (VisualMoonGroupContainer visualMoonGroup in visualMoonCatalogue.visualMoonGroupContainersList)
            {
                debugString += "VisualMoonGroup #" + counter + ": ";
                foreach (VisualMoonContainer visualMoonContainer in visualMoonGroup.visualMoonContainersList)
                    debugString += ", " + visualMoonContainer.currentExtendedLevel.NumberlessPlanetName;
                debugString += "\n";
                counter++;

            }
            Plugin.DebugLog(debugString + "dwadawdwadawdawdaw");
        }
        internal static VisualMoonGroupContainer InstansiateVisualMoonGroupContainer(ExtendedLevelGroup extendedLevelGroup, Vector2 galaxyOffset)
        {
            GameObject visualMoonGroupContainerObject = GameObject.Instantiate(visualMoonCatalogue.visualMoonGroupContainerPrefab, visualMoonCatalogue.transform);
            VisualMoonGroupContainer visualMoonGroupContainer = visualMoonGroupContainerObject.GetComponent<VisualMoonGroupContainer>();

            //Spawn Moons
            List<Vector3> lineRendererPositions = new List<Vector3>();
            List<ExtendedLevel> sortedExtendedLevels = extendedLevelGroup.extendedLevelsList.OrderBy(o => o.NumberlessPlanetName.Length).ToList();
            foreach (ExtendedLevel extendedLevel in sortedExtendedLevels)
            {
                VisualMoonContainer visualMoonContainer = InstansiateVisualMoonContainer(visualMoonGroupContainer, extendedLevel, galaxyOffset);
                visualMoonGroupContainer.visualMoonContainersList.Add(visualMoonContainer);
                visualMoonCatalogue.visualMoonContainersList.Add(visualMoonContainer);
                lineRendererPositions.Add(new Vector3(visualMoonContainer.spawnedMoonObject.transform.position.x, visualMoonCatalogue.visualMoonCatalogueSpawnPosition.y + 10, visualMoonContainer.spawnedMoonObject.transform.position.z));
            }

            //visualMoonGroupContainer.lineRenderer.transform.position = new Vector3(galaxyOffset.x, 10, galaxyOffset.y);
            visualMoonGroupContainer.lineRenderer.startWidth = visualMoonCatalogue.lineRendererLineWidth;
            visualMoonGroupContainer.lineRenderer.endWidth = visualMoonCatalogue.lineRendererLineWidth;
            visualMoonGroupContainer.lineRenderer.useWorldSpace = true;
            visualMoonGroupContainer.lineRenderer.positionCount = lineRendererPositions.Count;
            visualMoonGroupContainer.lineRenderer.SetPositions(lineRendererPositions.ToArray());



            return (visualMoonGroupContainer);
        }

        internal static VisualMoonContainer InstansiateVisualMoonContainer(VisualMoonGroupContainer visualMoonGroupContainer, ExtendedLevel extendedLevel, Vector2 galaxyOffset)
        {
            GameObject visualMoonContainerObject = GameObject.Instantiate(visualMoonCatalogue.visualMoonContainerPrefab, visualMoonGroupContainer.transform);
            VisualMoonContainer visualMoonContainer = visualMoonContainerObject.GetComponent<VisualMoonContainer>();
            visualMoonContainerObject.transform.position = new Vector3(galaxyOffset.x, visualMoonCatalogue.visualMoonCatalogueSpawnPosition.y + 15, galaxyOffset.y);

            Color color = new Color(21, 191, 0, 255);
            if (visualMoonCatalogue.useTagColors == true && TagColorDictionary.TryGetTagColor(extendedLevel.levelTags.ToArray(), out Color tagColor))
                color = tagColor;

            //Setup Moon
            Plugin.DebugLog("Instacing Prefab For: " + extendedLevel.NumberlessPlanetName);
            GameObject newPlanetObject = GameObject.Instantiate(extendedLevel.selectableLevel.planetPrefab, visualMoonContainer.transform);
            visualMoonContainer.spawnedMoonObject = newPlanetObject;
            visualMoonContainer.currentExtendedLevel = extendedLevel;
            Animator planetAnimator = newPlanetObject.GetComponent<Animator>();
            if (planetAnimator != null)
                UnityEngine.Object.Destroy(planetAnimator);


            float extendedLevelOffset = visualMoonCatalogue.extendedLevelOffset;
            if (visualMoonGroupContainer.visualMoonContainersList.Count % 2 == 0)
                newPlanetObject.transform.position += new Vector3(extendedLevelOffset * visualMoonGroupContainer.visualMoonContainersList.Count, 0, 0);
            else
                newPlanetObject.transform.position += new Vector3(extendedLevelOffset * visualMoonGroupContainer.visualMoonContainersList.Count, 0, extendedLevelOffset * visualMoonGroupContainer.visualMoonContainersList.Count);

            MeshRenderer meshRenderer = newPlanetObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                foreach (Material material in meshRenderer.materials)
                {
                    material.SetFloat("_UseEmissiveIntensity", 1f);
                    material.SetFloat("_AlbedoAffectEmissive", 1f);
                    material.SetFloat("_EmissiveIntensity", 1f);
                    material.SetFloat("_EmissiveIntensityUnit", 0f);
                    material.SetFloat("_EmissiveColorMode", 1f);
                    material.SetFloat("_EmissiveExposureWeight", 1f);
                    material.SetColor("_EmissiveColor", color);
                    material.SetColor("_EmissionColor", new Color(255, 255, 255, 255));
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }

            visualMoonContainer.spawnedMoonText.transform.position = newPlanetObject.transform.position;
            visualMoonContainer.spawnedMoonText.transform.Rotate(new Vector3(90, 0, 0));
            visualMoonContainer.spawnedMoonText.transform.position += new Vector3(visualMoonCatalogue.textOffsetX, 0, visualMoonCatalogue.textOffsetZ);

            visualMoonContainer.spawnedMoonText.text = extendedLevel.selectableLevel.PlanetName;
            visualMoonContainer.spawnedMoonText.color = color;
            visualMoonContainer.spawnedMoonText.fontSize = visualMoonCatalogue.textFontSize;

            return (visualMoonContainer);
        }

        internal static void AddRenderTextureToMoonsCatalogueNode()
        {
            terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            if (terminal != null)
            {
                Plugin.DebugLog("Found Terminal!");
                moonsCatalogueNode = terminal.terminalNodes.allKeywords[21].specialKeywordResult;
                if (moonsCatalogueNode != null)
                {
                    Plugin.DebugLog("Found Moons Catalogue TerminalNode!");
                    moonsCatalogueNode.displayTexture = visualMoonCatalogue.renderTexture;
                    moonsCatalogueNode.loadImageSlowly = true;
                    moonsCatalogueNode.persistentImage = true;
                }
            }


        }
    }
}
