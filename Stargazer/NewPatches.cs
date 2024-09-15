using HarmonyLib;
using LethalLevelLoader;
using Stargazer.Components;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Stargazer
{
    internal static class NewPatches
    {
        internal static Terminal Terminal;
        internal static TerminalNode MoonsCatalogueNode;

        internal static StarmapUIManager ActiveStarmapManager;
        internal static bool hasStarted;

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Start))]
        [HarmonyPostfix]
        internal static void RoundManagerStart_Postfix()
        {
            Plugin.DebugLog("Starting Stargazer!");

            if (TerminalManager.currentMoonsCataloguePage.ExtendedLevels.Count == 0)
                Plugin.DebugLog("No Levels Found!");

            if (hasStarted == true) return;

            Terminal = Object.FindFirstObjectByType<Terminal>();
            MoonsCatalogueNode = Terminal.terminalNodes.allKeywords[21].specialKeywordResult;
            InitializeUI();
            hasStarted = true;
        }

        internal static void InitializeUI()
        {
            Transform terminalParent = Terminal.terminalImageMask.transform;
            GameObject starmapManagerObject = GameObject.Instantiate(Assets.Manifest.StarmapUIManagerPrefab, terminalParent);
            ActiveStarmapManager = starmapManagerObject.GetComponent<StarmapUIManager>();
            ActiveStarmapManager.maskImage.fillAmount = 0;
        }


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.TextPostProcess))]
        [HarmonyPostfix]
        internal static void TerminalTextPostProcess_Postfix(TerminalNode node, ref string __result)
        {
            if (node == MoonsCatalogueNode)
            {
                ActiveStarmapManager.TryToggleUI(true);
                ActiveStarmapManager.SelectMoonGroup(null);
                ActiveStarmapManager.SelectMoon(null);
                __result = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
            }
            else if (ActiveStarmapManager.routeToGroupDict.TryGetValue(node, out MoonGroupUIVisualizer groupVisualizer))
            {
                //Plugin.DebugLog("Switching Active Moon To: " + ActiveStarmapManager.routeToMoonDict[node].controllingLevel.NumberlessPlanetName);
                ActiveStarmapManager.SelectMoon(ActiveStarmapManager.routeToMoonDict[node]);
                ActiveStarmapManager.SelectMoonGroup(groupVisualizer);
                __result = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";

            }
            else
                ActiveStarmapManager.TryToggleUI(false);

        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.BeginUsingTerminal))]
        [HarmonyPrefix]
        internal static void TerminalBeginUsingTerminal_Prefix()
        {
            ActiveStarmapManager.TryToggleUI(false);
            ActiveStarmapManager.Reset();
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.QuitTerminal))]
        [HarmonyPostfix]
        internal static void TerminalQuitTerminal_Postfix()
        {
            ActiveStarmapManager.TryToggleUI(false);
            ActiveStarmapManager.borderMask.fillAmount = 0;
        }

        internal static TerminalNode fixNode = null;
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.OnSubmit))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        internal static void TerminalOnSubmit_Prefix(Terminal __instance)
        {
            TerminalNode currentNode = __instance.currentNode;

            if (currentNode != null && currentNode.overrideOptions == true && ActiveStarmapManager.routeToGroupDict.ContainsKey(currentNode))
            {
                currentNode.overrideOptions = false;
                TerminalNode parsedNode = __instance.ParsePlayerSentence();
                if (parsedNode != null)
                    if (parsedNode == MoonsCatalogueNode || ActiveStarmapManager.routeToGroupDict.ContainsKey(parsedNode))
                        __instance.currentNode = MoonsCatalogueNode;
                currentNode.overrideOptions = true;
            }
        }
        /*
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.OnSubmit))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        internal static void TerminalOnSubmit_Postfix(Terminal __instance)
        {
            if (fixNode != null)
            {
                fixNode.acceptAnything = false;
                fixNode = null;
            }
        }*/


    }

    public struct NodeEdit
    {
        public TerminalNode node;
        public bool acceptAnything;
        public bool overrideOptions;

        public NodeEdit(TerminalNode newNode, bool newAccept, bool newOverride)
        {
            node = newNode;
            acceptAnything = newAccept;
            overrideOptions = newOverride;
        }
    }
}
