using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLevelLoader;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Stargazer
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(LethalLevelLoader.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "IAmBatby.Stargazer";
        public const string ModName = "Stargazer";
        public const string ModVersion = "1.0.0.0";

        public static Plugin Instance;

        internal static readonly Harmony Harmony = new Harmony(ModGUID);

        public static AssetBundle AssetBundle;

        internal static ManualLogSource logger;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            logger = Logger;

            Assets.LoadBundle();

            Logger.LogMessage("Succesfully Loaded Visual Moon Catalogue!");

            //Harmony.PatchAll(typeof(Patches));
            Harmony.PatchAll(typeof(NewPatches));
        }

        public static void DebugLog(string log)
        {
            logger.LogInfo(log);
        }

        public static void DebugLogError(string log)
        {
            logger.LogError(log);
        }
    }
}