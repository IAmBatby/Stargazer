using ES3Internal;
using LethalLevelLoader;
using Stargazer.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;

namespace Stargazer
{
    public enum VisualizerState { Default, Inactive, Active }
    public class StarmapUIManager : MonoBehaviour
    {
        public Image borderMask;
        public Image maskImage;
        public Image backgroundImage;

        public List<MoonGroupUIVisualizer> allMoonGroupVisualizers = new List<MoonGroupUIVisualizer>();
        public List<MoonUIVisualizer> allMoonVisualizers;

        internal static Dictionary<string, Color> starmapTags;

        //public List<MoonGroupUIVisualizer> allMoonGroupVisualizers = new List<MoonGroupUIVisualizer>();

        public Dictionary<Anchor, RectTransform> UIAnchorDictionary = new Dictionary<Anchor, RectTransform>();

        public static List<Vector2> randomGroupPositions = new List<Vector2>();

        internal bool UIActive;

        internal MoonGroupUIVisualizer activeGroupVisualizer;

        internal MoonUIVisualizer activeMoonVisualizer;

        internal List<TerminalNode> moonNodes = new List<TerminalNode>();

        internal Dictionary<TerminalNode, MoonGroupUIVisualizer> routeToGroupDict = new Dictionary<TerminalNode, MoonGroupUIVisualizer>();
        internal Dictionary<TerminalNode, MoonUIVisualizer> routeToMoonDict = new Dictionary<TerminalNode, MoonUIVisualizer>();
        internal float lerpTime = 0.1f;

        internal static Color InactiveColor { get; private set; } = new Color(1f, 1f, 1f, 0.1f);

        internal static float defaultTime = 0.05f;
        internal static float activeTime = 0.05f;
        internal float inactiveTime = 0.005f;

        private void Awake()
        {
            DisableUI();
            borderMask.fillAmount = 0;
            InitializeInfo();
            InitializeMoonGroups();
        }

        private void InitializeInfo()
        {
            UIAnchorDictionary = new Dictionary<Anchor, RectTransform>()
            {
                {Anchor.TopLeft, allMoonGroupVisualizers[0].RectTransform },
                {Anchor.TopCenter, allMoonGroupVisualizers[1].RectTransform },
                {Anchor.TopRight, allMoonGroupVisualizers[2].RectTransform },
                {Anchor.CenterLeft, allMoonGroupVisualizers[3].RectTransform },
                {Anchor.Center, allMoonGroupVisualizers[4].RectTransform },
                {Anchor.CenterRight, allMoonGroupVisualizers[5].RectTransform },
                {Anchor.BottomLeft, allMoonGroupVisualizers[6].RectTransform },
                {Anchor.BottomCenter, allMoonGroupVisualizers[7].RectTransform },
                {Anchor.BottomRight, allMoonGroupVisualizers[8].RectTransform }
            };

            float xMax = 35;
            float yMax = 35;
            randomGroupPositions = new List<Vector2>()
            {
                new (-xMax,-yMax),
                new (-xMax,yMax),
                new (0,0),
                new (xMax,-yMax),
                new (xMax,yMax),
            };

            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                moonNodes.Add(extendedLevel.RouteNode);
        }

        private void InitializeMoonGroups()
        {
            ExtendedLevelGroup[] extendedLevelGroups = GetMoonGroups();

            allMoonVisualizers = new List<MoonUIVisualizer>();

            starmapTags = Assets.Manifest.colorManifest.CreateDictionary();

            for (int i = 0; i < extendedLevelGroups.Length; i++)
            {
                if (extendedLevelGroups[i] == null) continue;
                Dictionary<ExtendedLevel, Vector2> randomOffsets = GetRandomOffsets(extendedLevelGroups[i].extendedLevelsList);

                MoonUIVisualizer previousLevel = null;
                foreach (ExtendedLevel extendedLevel in extendedLevelGroups[i].extendedLevelsList)
                {
                    MoonUIVisualizer moonUIVisualizer = GameObject.Instantiate(Assets.Manifest.MoonUIVisualizerPrefab, allMoonGroupVisualizers[i].RectTransform).GetComponent<MoonUIVisualizer>();
                    moonUIVisualizer.transform.Rotate(0, 180, 0);
                    moonUIVisualizer.Initialize(extendedLevel);

                    if (extendedLevelGroups[i].extendedLevelsList.Count > 1)
                        OffsetMoon(allMoonGroupVisualizers[i].RectTransform, moonUIVisualizer, randomOffsets[extendedLevel]);

                    if (previousLevel != null)
                    {
                        MoonLine moonLine = GameObject.Instantiate(Assets.Manifest.MoonLine, allMoonGroupVisualizers[i].RectTransform).GetComponent<MoonLine>();
                        moonLine.Apply(previousLevel, moonUIVisualizer);
                        allMoonGroupVisualizers[i].allMoonLines.Add(moonLine);
                    }

                    routeToGroupDict.Add(extendedLevel.RouteNode, allMoonGroupVisualizers[i]);
                    routeToMoonDict.Add(extendedLevel.RouteNode, moonUIVisualizer);

                    previousLevel = moonUIVisualizer;
                    allMoonGroupVisualizers[i].allMoonVisualizers.Add(moonUIVisualizer);
                    allMoonVisualizers.Add(moonUIVisualizer);
                }
            }
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

        internal ExtendedLevelGroup[] GetMoonGroups()
        {
            ExtendedLevelGroup[] returnGroups = new ExtendedLevelGroup[9];
            List<ExtendedLevelGroup> extendedLevelGroups = new List<ExtendedLevelGroup>(TerminalManager.defaultMoonsCataloguePage.ExtendedLevelGroups);
            ExtendedLevelGroup gordionGroup = new ExtendedLevelGroup(new List<ExtendedLevel>() { PatchedContent.ExtendedLevels[3] });

            int index = 0;
  
            for (int i = 0; i < extendedLevelGroups.Count; i++)
            {
                if (index == returnGroups.Length) break;
                if (index != 4)
                    returnGroups[i] = extendedLevelGroups[i];
                else
                    index++;
                index++;
            }
            returnGroups[4] = gordionGroup;

            for (int i = 0; i < returnGroups.Length; i++)
                if (i != 4 && returnGroups[i] != null)
                    foreach (ExtendedLevel level in new List<ExtendedLevel>(returnGroups[i].extendedLevelsList))
                        if (level.NumberlessPlanetName.Contains("Gordion") || level.NumberlessPlanetName.Contains("Liquidation"))
                            returnGroups[i].extendedLevelsList.Remove(level);

            return (returnGroups);
        }

        internal void TryToggleUI(bool value)
        {
            if (value == true && UIActive == false)
                EnableUI();
            else if (value == false && UIActive == true)
                DisableUI();
        }

        private void EnableUI()
        {
            UIActive = true;
        }

        private void DisableUI()
        {
            UIActive = false;
        }

        internal void SelectMoon(MoonUIVisualizer visualizer)
        {
            if (visualizer == null)
                foreach (MoonUIVisualizer moon in allMoonVisualizers)
                    moon.SetVisualizerState(VisualizerState.Default);
            else
            {
                foreach (MoonUIVisualizer moon in allMoonVisualizers)
                    moon.SetVisualizerState(VisualizerState.Inactive);

                activeMoonVisualizer = visualizer;
                activeMoonVisualizer.SetVisualizerState(VisualizerState.Active);
            }
        }

        internal void SelectMoonGroup(MoonGroupUIVisualizer moonVisualizer)
        {
            if (moonVisualizer == null)
                foreach (MoonGroupUIVisualizer moonGroup in allMoonGroupVisualizers)
                    moonGroup.SetVisualizerState(VisualizerState.Default);
            else
            {
                foreach (MoonGroupUIVisualizer moonGroup in allMoonGroupVisualizers)
                    moonGroup.SetVisualizerState(VisualizerState.Inactive);

                activeGroupVisualizer = moonVisualizer;
                activeGroupVisualizer.SetVisualizerState(VisualizerState.Active);
            }
        }

        internal void Reset()
        {
            activeGroupVisualizer = null;
            activeMoonVisualizer = null;

            foreach (MoonGroupUIVisualizer moonGroup in allMoonGroupVisualizers)
                moonGroup.Reset();
            foreach (MoonUIVisualizer moon in allMoonVisualizers)
                moon.Reset();
        }

        private void Update()
        {
            if (UIActive == true && borderMask.fillAmount < 1f)
                borderMask.fillAmount = Mathf.Lerp(borderMask.fillAmount, 1f, 1 - Mathf.Pow(lerpTime, Time.deltaTime));
            else if (UIActive == false && borderMask.fillAmount > 0f)
                borderMask.fillAmount = Mathf.Lerp(borderMask.fillAmount, 0f, 1 - Mathf.Pow(lerpTime, Time.deltaTime));

            foreach (MoonGroupUIVisualizer moonGroup in allMoonGroupVisualizers)
            {
                if (moonGroup == activeGroupVisualizer)
                    LerpMoonGroupToActive(moonGroup);
                else if (activeGroupVisualizer != null)
                    LerpMoonGroupToInactive(moonGroup);
                else
                    LerpMoonGroupToDefaults(moonGroup);
            }

            foreach (MoonUIVisualizer moon in allMoonVisualizers)
            {
                if (moon == activeMoonVisualizer)
                    LerpMoonToActive(moon);
                else if (activeMoonVisualizer != null)
                    LerpMoonToInactive(moon);
                else
                    LerpMoonToDefaults(moon);
            }
        }

        internal void ResetMoonGroupUI(MoonGroupUIVisualizer moonGroup)
        {
            moonGroup.RectTransform.offsetMin = moonGroup.defaultOffsetMin;
            moonGroup.RectTransform.offsetMax = moonGroup.defaultOffsetMax;
            moonGroup.RectTransform.anchorMin = moonGroup.defaultAnchorMin;
            moonGroup.RectTransform.localPosition = moonGroup.defaultLocalPosition;
            moonGroup.RectTransform.localScale = Vector3.one;

            foreach (MoonUIVisualizer moonUIVisualizer in moonGroup.allMoonVisualizers)
                moonUIVisualizer.ApplyDefaultColor();
        }

        private void LerpMoonGroupToDefaults(MoonGroupUIVisualizer moonGroup)
        {
            RectTransform rect = moonGroup.RectTransform;

            rect.offsetMin = Lerp(rect.offsetMin, moonGroup.defaultOffsetMin, defaultTime);
            rect.offsetMax = Lerp(rect.offsetMax, moonGroup.defaultOffsetMax, defaultTime);

            rect.localPosition = Lerp(rect.localPosition, moonGroup.defaultLocalPosition, defaultTime);
            rect.localScale = Lerp(rect.localScale, Vector3.one, defaultTime);
        }

        private void LerpMoonGroupToActive(MoonGroupUIVisualizer moonGroup)
        {
            RectTransform rect = moonGroup.RectTransform;

            rect.offsetMin = Lerp(rect.offsetMin, Vector2.zero, activeTime);
            rect.offsetMax = Lerp(rect.offsetMax, Vector2.zero, activeTime);

            rect.localPosition = Lerp(rect.localPosition, Vector3.zero, activeTime);
            rect.localScale = Lerp(rect.localScale, new Vector3(1.85f,1.85f,1f), activeTime);
        }

        private void LerpMoonGroupToInactive(MoonGroupUIVisualizer moonGroup)
        {
            moonGroup.RectTransform.localScale = Lerp(moonGroup.RectTransform.localScale, Vector3.zero, inactiveTime);
        }

        private void LerpMoonToDefaults(MoonUIVisualizer moonVisualizer)
        {
            moonVisualizer.ApplyColor(Color.Lerp(moonVisualizer.ActiveColor, moonVisualizer.DefaultColor, activeTime / 2));
        }

        private void LerpMoonToActive(MoonUIVisualizer moonVisualizer)
        {
            moonVisualizer.ApplyColor(Color.Lerp(moonVisualizer.ActiveColor, moonVisualizer.DefaultColor, activeTime / 2));
        }

        private void LerpMoonToInactive(MoonUIVisualizer moonVisualizer)
        {
            moonVisualizer.ApplyColor(Color.Lerp(moonVisualizer.ActiveColor, InactiveColor, inactiveTime / 2));
        }

        internal static float Lerp(float current, float target, float t) => (Mathf.Lerp(current, target, 1 - Mathf.Pow(t, Time.deltaTime)));

        internal static Vector2 Lerp(Vector2 current, Vector2 target, float t) => new Vector2(Lerp(current.x, target.x, t), Lerp(current.y, target.y, t));

        internal static Vector3 Lerp(Vector3 current, Vector3 target, float t) => new Vector3(Lerp(current.x, target.x, t), Lerp(current.y, target.y, t), Lerp(current.z, target.z, t));
    }
}
