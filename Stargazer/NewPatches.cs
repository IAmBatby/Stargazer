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
            if (hasStarted == true) return;

            if (TerminalManager.currentMoonsCataloguePage.ExtendedLevels.Count == 0)
                Plugin.DebugLog("No Levels Found!");
            else
                Plugin.DebugLog("Starting Stargazer!");


            Terminal = Object.FindFirstObjectByType<Terminal>();
            MoonsCatalogueNode = Terminal.terminalNodes.allKeywords[21].specialKeywordResult;
            ActiveStarmapManager = GameObject.Instantiate(Assets.Manifest.StarmapUIManagerPrefab, Terminal.terminalImageMask.transform).GetComponent<StarmapUIManager>();
            hasStarted = true;
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.TextPostProcess))]
        [HarmonyPostfix]
        internal static void TerminalTextPostProcess_Postfix(TerminalNode node, ref string __result)
        {
            if (node == MoonsCatalogueNode && ActiveStarmapManager.UIActive == false)
                ActiveStarmapManager.ToggleUI(true);
            else if (node == MoonsCatalogueNode && ActiveStarmapManager == true)
            {
                ActiveStarmapManager.SetActiveMoon(null);
                ActiveStarmapManager.SetActiveMoonGroup(null);
            }
            else if (ActiveStarmapManager.RouteMoonDictionary.TryGetValue(node, out MoonUIVisualizer moon))
            {
                if (moon.CurrentLevel.IsRouteLocked == false && moon.CurrentLevel.IsRouteHidden == true)
                {
                    HUDManager.Instance.DisplayTip("New Route Discovered", moon.CurrentLevel.SelectableLevel.PlanetName, false, false);
                    moon.CurrentLevel.IsRouteHidden = false;
                    moon.RefreshVisualInfo();
                }
                ActiveStarmapManager.SetActiveMoon(ActiveStarmapManager.RouteMoonDictionary[node]);
            }
            else
                ActiveStarmapManager.ToggleUI(false);

            if (ActiveStarmapManager.UIActive == true)
                __result = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.BeginUsingTerminal))]
        [HarmonyPrefix]
        internal static void TerminalBeginUsingTerminal_Prefix()
        {
            ActiveStarmapManager.ToggleUI(false);
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.OnSubmit))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void TerminalOnSubmit_Prefix(Terminal __instance)
        {
            TerminalNode currentNode = __instance.currentNode;

            if (currentNode != null && currentNode.overrideOptions == true && ActiveStarmapManager.RouteGroupDictionary.ContainsKey(currentNode))
            {
                currentNode.overrideOptions = false;
                if (IsRelevantNode(__instance.ParsePlayerSentence()))
                    __instance.currentNode = MoonsCatalogueNode;
                currentNode.overrideOptions = true;
            }
        }

        internal static bool IsRelevantNode(TerminalNode node)
        {
            if (node != null && (node == MoonsCatalogueNode || ActiveStarmapManager.RouteGroupDictionary.ContainsKey(node)))
                return (true);
            return (false);
        }
    }
}
