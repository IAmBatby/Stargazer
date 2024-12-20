﻿using LethalLevelLoader;
using Stargazer.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer
{
    public enum VisualizerState { Default, Inactive, Active }
    public class StarmapUIManager : MonoBehaviour
    {
        public static StarmapUIManager Instance;
        [Header("Serialized References")]
        [SerializeField] private Image borderMask;
        [SerializeField] private Image maskImage;
        [SerializeField] private Image backgroundImage;

        [SerializeField] private MoonDataVisualiser moonDataVisualiser;

        ///// Interal References /////

        internal Dictionary<TerminalNode, MoonGroupUIVisualizer> RouteGroupDictionary { get; private set; } = new Dictionary<TerminalNode, MoonGroupUIVisualizer>();
        internal Dictionary<TerminalNode, MoonUIVisualizer> RouteMoonDictionary { get; private set; } = new Dictionary<TerminalNode, MoonUIVisualizer>();

        internal List<TerminalNode> MoonNodes { get; private set; } = new List<TerminalNode>();
        internal Dictionary<TerminalNode, ExtendedLevel> SimulateNodeDict = new Dictionary<TerminalNode, ExtendedLevel>();

        internal Dictionary<TerminalNode, NodeType> NodeTypeDict = new Dictionary<TerminalNode, NodeType>();

        internal static Dictionary<string, Color> StarmapTags { get; private set; }

        internal static Color InactiveColor { get; private set; } = new Color(1f, 1f, 1f, 0.1f);

        internal List<ExtendedLevelGroup> extendedLevelGroups = new List<ExtendedLevelGroup>();

        internal bool UIActive { get; private set; }
        internal bool HasInitialized { get; private set; }

        internal MoonGroupUIVisualizer ActiveGroup
        {
            get
            {
                MoonGroupUIVisualizer value = null;
                foreach (MoonGroupUIVisualizer moonGroup in allMoonGroupVisualizers)
                    if (moonGroup.CurrentState == VisualizerState.Active)
                        value = moonGroup;
                return (value);
            }
        }
        internal MoonUIVisualizer ActiveMoon
        {
            get
            {
                MoonUIVisualizer value = null;
                foreach (MoonUIVisualizer moon in allMoonVisualizers)
                    if (moon.CurrentState == VisualizerState.Active)
                        value = moon;
                return (value);
            }
        }

        ///// Private References /////

        [SerializeField] private List<StarmapLayout> allLayouts = new List<StarmapLayout>();
        
        [SerializeField] private List<MoonGroupUIVisualizer> allMoonGroupVisualizers = new List<MoonGroupUIVisualizer>();
        private List<MoonUIVisualizer> allMoonVisualizers = new List<MoonUIVisualizer>();
        private float uiScrollLerpTime = 0.1f;
        private List<Vector2> randomGroupPositions = new List<Vector2>();

        [SerializeField] private DynamicGridLayout dynamicGridLayout;

        private void Awake()
        {
            Instance = this;
            enabled = true;
            //moonDataVisualiser.gameObject.SetActive(false);
            borderMask.fillAmount = 0;
            maskImage.fillAmount = 0;
            ToggleUI(false);
            InitializeInfo();
            PopulateLevelGroups();
            InitializeMoonGroups();
            InitializeMoons();

            NodeTypeDict.Add(NewPatches.MoonsCatalogueNode, NodeType.Catalogue);
            HasInitialized = true;
        }

        public void SetNewInfo(string newTitle = null, string newDescription = null)
        {
            if (!string.IsNullOrEmpty(newTitle))
                moonDataVisualiser.titleText.SetText(newTitle);
            if (!string.IsNullOrEmpty(newDescription))
                moonDataVisualiser.descriptionText.SetText(newDescription);
        }

        private void InitializeInfo()
        {
            float xMax = 35;
            float yMax = 35;
            randomGroupPositions = new List<Vector2>()
            {
                new (-xMax,-yMax), new (-xMax,yMax), new (0,0), new (xMax,-yMax), new (xMax,yMax),
            };

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                MoonNodes.Add(extendedLevel.RouteNode);

            StarmapTags = Assets.Manifest.colorManifest.CreateDictionary();
        }

        private void InitializeMoonGroups()
        {
            List<MoonGroupUIVisualizer> newMoonGroups = new List<MoonGroupUIVisualizer>();
            foreach (ExtendedLevelGroup extendedLevelGroup in extendedLevelGroups)
            {
                MoonGroupUIVisualizer moonGroup = GameObject.Instantiate(Assets.Manifest.MoonGroupUIVisualizerPrefab, dynamicGridLayout.transform).GetComponent<MoonGroupUIVisualizer>();
                newMoonGroups.Add(moonGroup);
            }

            allMoonGroupVisualizers = new List<MoonGroupUIVisualizer>(newMoonGroups);

            int squareValue = Mathf.RoundToInt(Mathf.Sqrt(extendedLevelGroups.Count));
            if (extendedLevelGroups.Count > squareValue)
                squareValue = Mathf.RoundToInt(Mathf.Sqrt(extendedLevelGroups.Count + squareValue));
            dynamicGridLayout.RefreshDynamicGridLayout(squareValue);
        }

        private void InitializeMoons()
        {
            allMoonVisualizers = new List<MoonUIVisualizer>();

            for (int i = 0; i < extendedLevelGroups.Count; i++)
            {
                if (extendedLevelGroups[i] == null) continue;
                //if (allMoonGroupVisualizers[i] == null) continue;

                MoonGroupUIVisualizer moonGroup = allMoonGroupVisualizers[i];
                moonGroup.RectTransform = moonGroup.GetComponent<RectTransform>();
                Dictionary<ExtendedLevel, Vector2> randomOffsets = GetRandomOffsets(extendedLevelGroups[i].extendedLevelsList);

                if (extendedLevelGroups[i].extendedLevelsList.Count > 1)
                    for (int j = 0; j < extendedLevelGroups[i].extendedLevelsList.Count; j++)
                        InitializeMoonLine(moonGroup);

                for (int j = 0; j < extendedLevelGroups[i].extendedLevelsList.Count; j++)
                {
                    ExtendedLevel extendedLevel = extendedLevelGroups[i].extendedLevelsList[j];

                    MoonUIVisualizer moon = InitializeMoonVisualizer(moonGroup, extendedLevel, extendedLevelGroups[i].extendedLevelsList.Count, randomOffsets[extendedLevel]);

                    if (j > 0 && extendedLevelGroups[i].extendedLevelsList.Count > 1)
                        moonGroup.allMoonLines[j].Apply(moonGroup.allMoonVisualizers[j], moonGroup.allMoonVisualizers[j - 1]);

                    if (extendedLevel.RouteNode != null)
                    {
                        RouteGroupDictionary.Add(extendedLevel.RouteNode, moonGroup);
                        RouteMoonDictionary.Add(extendedLevel.RouteNode, moon);
                        SimulateNodeDict.Add(extendedLevel.SimulateNode, extendedLevel);
                    }

                    allMoonVisualizers.Add(moon);
                }
                if (extendedLevelGroups[i].extendedLevelsList.Count > 1)
                    moonGroup.allMoonLines[0].Apply(moonGroup.allMoonVisualizers.First(), moonGroup.allMoonVisualizers.Last());
            }

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                if (extendedLevel.RouteNode == null || extendedLevel.InfoNode == null || extendedLevel.SimulateNode == null)
                {
                    Plugin.DebugLogError(extendedLevel.NumberlessPlanetName + " Has Invalid Nodes!");
                    continue;
                }
                NodeTypeDict.Add(extendedLevel.RouteNode, NodeType.Route);
                NodeTypeDict.Add(extendedLevel.RouteConfirmNode, NodeType.RouteConfirm);
                NodeTypeDict.Add(extendedLevel.InfoNode, NodeType.Info);
                NodeTypeDict.Add(extendedLevel.SimulateNode, NodeType.Simulate);
            }

            string debugString = "Finished Initializing Stargazer, Results Below: " + "\n\n";

            foreach (MoonGroupUIVisualizer moonGroup in  allMoonGroupVisualizers)
            {
                debugString += allMoonGroupVisualizers.IndexOf(moonGroup) + " : ";

                foreach (MoonUIVisualizer moon in moonGroup.allMoonVisualizers)
                    debugString += moon.CurrentLevel.NumberlessPlanetName + ", ";

                debugString += "\n";
            }

            Plugin.DebugLog(debugString);
        }

        private MoonUIVisualizer InitializeMoonVisualizer(MoonGroupUIVisualizer moonGroup, ExtendedLevel extendedLevel, int groupCount, Vector2 offset)
        {
            MoonUIVisualizer moonUIVisualizer = GameObject.Instantiate(Assets.Manifest.MoonUIVisualizerPrefab, moonGroup.RectTransform).GetComponent<MoonUIVisualizer>();
            moonUIVisualizer.transform.Rotate(0, 180, 0);
            moonUIVisualizer.Initialize(extendedLevel, moonGroup);
            moonUIVisualizer.gameObject.name = "MoonUIVisualizer (" + moonUIVisualizer.CurrentLevel.NumberlessPlanetName + ")";

            if (groupCount > 1)
                OffsetMoon(moonGroup.RectTransform, moonUIVisualizer, offset);

            moonGroup.allMoonVisualizers.Add(moonUIVisualizer);

            return (moonUIVisualizer);
        }

        private void InitializeMoonLine(MoonGroupUIVisualizer moonGroup)
        {
            MoonLine moonLine = GameObject.Instantiate(Assets.Manifest.MoonLine, moonGroup.RectTransform).GetComponent<MoonLine>();
            moonGroup.allMoonLines.Add(moonLine);
        }

        private Dictionary<ExtendedLevel, Vector2> GetRandomOffsets(List<ExtendedLevel> extendedLevels)
        {
            Dictionary<ExtendedLevel, Vector2> returnDict = new Dictionary<ExtendedLevel, Vector2>();

            int count = 0;
            foreach (ExtendedLevel level in extendedLevels)
                count += level.NumberlessPlanetName.Length;

            System.Random newRandom = new System.Random(count);
            List<Vector2> randomOffsets = new List<Vector2>(randomGroupPositions);
            foreach (ExtendedLevel level in extendedLevels)
            {
                Vector2 newOffset = randomOffsets[newRandom.Next(0, randomOffsets.Count)];
                randomOffsets.Remove(newOffset);
                returnDict.Add(level, newOffset);
            }

            return (returnDict);
        }

        private void OffsetMoon(RectTransform parent, MoonUIVisualizer moon, Vector2 offset)
        {
            RectTransform rect = moon.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, offset, 0.75f);
        }

        internal void PopulateLevelGroups()
        {
            List<ExtendedLevelGroup> returnGroups = new List<ExtendedLevelGroup>();
            ExtendedLevelGroup gordionGroup = new ExtendedLevelGroup(new List<ExtendedLevel>() { PatchedContent.ExtendedLevels[3] });

            List<ExtendedLevelGroup> allExtendedLevelGroups = new List<ExtendedLevelGroup>();
            List<ExtendedLevel> allLevels = new List<ExtendedLevel>();

            foreach (ExtendedLevelGroup currentGroup in TerminalManager.defaultMoonsCataloguePage.ExtendedLevelGroups.Concat(TerminalManager.currentMoonsCataloguePage.ExtendedLevelGroups))
            {
                bool uniqueGroup = false;
                foreach (ExtendedLevel level in currentGroup.extendedLevelsList)
                    if (!allLevels.Contains(level))
                    {
                        uniqueGroup = true;
                        break;
                    }
                if (uniqueGroup == true)
                {
                    allExtendedLevelGroups.Add(currentGroup);
                    foreach (ExtendedLevel level in currentGroup.extendedLevelsList)
                        allLevels.Add(level);
                }
            }

            for (int i = 0; i < allExtendedLevelGroups.Count; i++)
                if (i != 4 && allExtendedLevelGroups[i] != null)
                    foreach (ExtendedLevel level in new List<ExtendedLevel>(allExtendedLevelGroups[i].extendedLevelsList))
                        if (level.NumberlessPlanetName.Contains("Gordion") || level.NumberlessPlanetName.Contains("Liquidation"))
                            allExtendedLevelGroups[i].extendedLevelsList.Remove(level);

            allExtendedLevelGroups.Insert(4, gordionGroup);

            extendedLevelGroups = new List<ExtendedLevelGroup>(allExtendedLevelGroups);
        }

        internal void ToggleUI(bool value)
        {
            bool oldValue = UIActive;
            UIActive = value;

            if (UIActive == true && oldValue == false)
                if (HasInitialized)
                    ResetVisualizers();

            if (UIActive == false && oldValue == true)
            {
                borderMask.fillAmount = 0;
                maskImage.fillAmount = 0;
            }
        }

        internal void SetActiveMoon(MoonUIVisualizer newMoon)
        {
            if (newMoon == null)
                ToggleAllMoonStates(VisualizerState.Default, false);
            else
            {
                ToggleAllMoonStates(VisualizerState.Inactive, false);
                newMoon.SetVisualizerState(VisualizerState.Active, false);
                SetActiveMoonGroup(newMoon.CurrentMoonGroup);
            }
        }

        internal void SetActiveMoonGroup(MoonGroupUIVisualizer newMoonGroup)
        {
            if (newMoonGroup == null)
                ToggleAllMoonGroupStates(VisualizerState.Default, false);
            else
            {
                ToggleAllMoonGroupStates(VisualizerState.Inactive, false);
                newMoonGroup.SetVisualizerState(VisualizerState.Active, false);
            }
        }

        internal void ResetVisualizers()
        {
            dynamicGridLayout.RefreshLayout();
            ToggleAllMoonGroupStates(VisualizerState.Default, true);
            ToggleAllMoonStates(VisualizerState.Default, true);
        }

        private void ToggleAllMoonGroupStates(VisualizerState newState, bool applyOnSet)
        {
            foreach (MoonGroupUIVisualizer moonGroup in allMoonGroupVisualizers)
                moonGroup.SetVisualizerState(newState, applyOnSet);
        }

        private void ToggleAllMoonStates(VisualizerState newState, bool applyOnSet)
        {
            foreach (MoonUIVisualizer moon in allMoonVisualizers)
                moon.SetVisualizerState(newState, applyOnSet);
        }

        private void Update()
        {
            if (UIActive == true && borderMask.fillAmount < 1f)
                borderMask.fillAmount = Mathf.Lerp(borderMask.fillAmount, 1f, 1 - Mathf.Pow(uiScrollLerpTime, Time.deltaTime));
            else if (UIActive == false && borderMask.fillAmount > 0f)
                borderMask.fillAmount = Mathf.Lerp(borderMask.fillAmount, 0f, 1 - Mathf.Pow(uiScrollLerpTime, Time.deltaTime));
        }
    }
}
