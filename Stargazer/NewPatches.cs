using HarmonyLib;
using LethalLevelLoader;
using Stargazer.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Audio.Handle;
using Object = UnityEngine.Object;

namespace Stargazer
{
    internal enum NodeType { Irrelevant, Catalogue, Route, RouteConfirm, Info, Simulate }

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
            NodeType newNodeType = IdentifyNode(node);

            Plugin.DebugLog(node.name + " - " + newNodeType);

            if (newNodeType == NodeType.Irrelevant)
                ActiveStarmapManager.ToggleUI(false);
            else if (newNodeType == NodeType.Catalogue)
            {
                if (ActiveStarmapManager.UIActive == false)
                    ActiveStarmapManager.ToggleUI(true);
                else
                {
                    ActiveStarmapManager.SetActiveMoon(null);
                    ActiveStarmapManager.SetActiveMoonGroup(null);
                }
            }
            else if (newNodeType == NodeType.Route)
            {
                MoonUIVisualizer moon = ActiveStarmapManager.RouteMoonDictionary[node];
                if (moon.CurrentLevel.IsRouteLocked == false && moon.CurrentLevel.IsRouteHidden == true)
                {
                    HUDManager.Instance.DisplayTip("New Route Discovered", moon.CurrentLevel.SelectableLevel.PlanetName, false, false);
                    moon.CurrentLevel.IsRouteHidden = false;
                    moon.RefreshVisualInfo();
                }
                ActiveStarmapManager.SetActiveMoon(moon);
                ActiveStarmapManager.SetNewInfo(newDescription: __result);
            }
            else if (newNodeType == NodeType.Simulate)
                ActiveStarmapManager.SetNewInfo(newDescription: TerminalManager.GetSimulationResultsText(ActiveStarmapManager.SimulateNodeDict[node]));
            else if (newNodeType == NodeType.Info)
                ActiveStarmapManager.SetNewInfo(newDescription: ActiveStarmapManager.ActiveMoon.CurrentLevel.SelectableLevel.LevelDescription);
            else if (newNodeType == NodeType.RouteConfirm)
                ActiveStarmapManager.SetNewInfo(newDescription: __result);
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
            if (currentNode == null) return;
            
            NodeType nodeType = IdentifyNode(currentNode);
            if (nodeType != NodeType.Irrelevant)
            {
                bool previousOverridesOptionsValue = currentNode.overrideOptions;

                currentNode.overrideOptions = false;
                if (IdentifyNode(__instance.ParsePlayerSentence()) != NodeType.Irrelevant)
                    __instance.currentNode = MoonsCatalogueNode;

                currentNode.overrideOptions = previousOverridesOptionsValue;
            }
        }

        
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.ParseWordOverrideOptions))]
        [HarmonyPostfix, HarmonyPriority(Priority.First)]
        internal static void TerminalParseWordOverrideOptions_Prefix(string playerWord, CompatibleNoun[] options, ref TerminalNode __result)
        {
            if (ActiveStarmapManager.ActiveMoon == null) return;
            return;
            if (IdentifyNode(Terminal.currentNode) != NodeType.Irrelevant || IdentifyNode(Terminal.currentNode) != NodeType.Catalogue)
            {
                if (playerWord == "simulate")
                    __result = ActiveStarmapManager.ActiveMoon.CurrentLevel.SimulateNode;
            }


            string debugString = "OverrideOptions Prefix: " + playerWord + "\n";
            foreach (CompatibleNoun noun in options)
            {
                if (noun.noun != null)
                    debugString += noun.noun.name + " - ";
                else
                    debugString += "(Null) - ";

                if (noun.result != null)
                    debugString += noun.result.name + "\n";
                else
                    debugString += "(Null) \n";
            }
            Plugin.DebugLog(debugString);
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.ParsePlayerSentence))]
        [HarmonyPostfix, HarmonyPriority(Priority.First)]
        internal static void TerminalParsePlayerSentence_Postfix(ref TerminalNode __result)
        {
            Plugin.DebugLog("ParsePlayerSentence Prefix: CurrentNode Is: " + Terminal.currentNode?.name + " , Override Options Is: " + Terminal.currentNode?.overrideOptions);

            if (ActiveStarmapManager.ActiveMoon == null) return;

            string strippedText = Terminal.screenText.text.Substring(Terminal.screenText.text.Length - Terminal.textAdded);
            strippedText = Terminal.RemovePunctuation(strippedText);
            string[] array = strippedText.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (array.Length == 1)
            {
                if (array[0] == "info")
                    __result = ActiveStarmapManager.ActiveMoon.CurrentLevel.InfoNode;
                else if (array[0] == "simulate")
                    __result = ActiveStarmapManager.ActiveMoon.CurrentLevel.SimulateNode;
                else if (array[0] == "route")
                    __result = ActiveStarmapManager.ActiveMoon.CurrentLevel.RouteNode;
            }
            /*
            if (array.Length == 1 && array[0] == "info")
                __result = ActiveStarmapManager.ActiveMoon.CurrentLevel.InfoNode;
            else if (array.Length == 1 && array[0] == "simulate")
                __result = ActiveStarmapManager.ActiveMoon.CurrentLevel.InfoNode;
            */
        }
        
        internal static NodeType IdentifyNode(TerminalNode node)
        {
            if (ActiveStarmapManager.NodeTypeDict.TryGetValue(node, out NodeType returnType))
                return (returnType);
            return (NodeType.Irrelevant);
        }
    }
}
