using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    internal static class Assets
    {
        internal static AssetBundle Bundle;
        internal static AssetManifest Manifest;

        internal static void LoadBundle()
        {
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Plugin.DebugLog("Trying To Load AssetBundle At " + sAssemblyLocation);
            Plugin.DebugLog("FilePath Should Be: " + Path.Combine(sAssemblyLocation, "stargazerbundle"));
            Bundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "stargazerbundle"));
            if (Bundle == null)
                Plugin.DebugLogError("Could Not Find AssetBundle!");
            else
            {
                Plugin.DebugLog("AssetBundle Found!");
                Manifest = Bundle.LoadAllAssets<AssetManifest>()[0];
            }
        }
    }
}
