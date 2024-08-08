using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLevelLoader;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace VisualMoonCatalogue
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(LethalLevelLoader.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "visualmooncatalogue";
        public const string ModName = "VisualMoonCatalogue";
        public const string ModVersion = "1.0.0.0";

        public static Plugin Instance;

        internal static readonly Harmony Harmony = new Harmony(ModGUID);

        public static AssetBundle AssetBundle;

        internal static BepInEx.Logging.ManualLogSource logger;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            logger = Logger;
            
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DebugLog("Trying To Load AssetBundle At " + sAssemblyLocation);
            DebugLog("FilePath Should Be: " + Path.Combine(sAssemblyLocation, "visualmooncatalogue"));
            AssetBundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "visualmooncatalogue"));
            if (AssetBundle == null)
                DebugLogError("Could Not Find AssetBundle!");
            else
                DebugLog("AssetBundle Found!");

            GameObject[] prefabs = AssetBundle.LoadAllAssets<GameObject>();
            foreach (GameObject prefab in prefabs)
            {
                DebugLog("Found Prefab: " + prefab.name);
                if (prefab.TryGetComponent(out VisualMoonCatalogueContainer visualMoonCatalogueContainer))
                {
                    DebugLog("Found VisualMoonCatalogueContainer Prefab!");
                    Patches.visualMoonCatalogueContainerPrefab = prefab;
                }
            }

            /*VisualMoonGroupContainer[] visualMoonGroupContainers = AssetBundle.LoadAllAssets<VisualMoonGroupContainer>();
            if (visualMoonGroupContainers[0] != null)
            {
                Logger.LogDebug("Found VisualMoonGroupContainer Prefab!");
                Patches.visualMoonCatalogueContainerPrefab = visualMoonGroupContainers[0].gameObject;
            }*/
            Logger.LogMessage("Succesfully Loaded Visual Moon Catalogue!");
            
            Harmony.PatchAll(typeof(Patches));
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