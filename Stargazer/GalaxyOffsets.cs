using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    public enum SolarSystemSize { Three, Four, Eight }
    public enum Anchor { TopLeft, TopCenter, TopRight, CenterLeft, Center, CenterRight, BottomLeft, BottomCenter, BottomRight }
    public static class GalaxyOffsets
    {
        public static SolarSystemSize currentSolarSystemSize;

        public static float distance = 140;
        public static Dictionary<Anchor, Vector2> anchorDictionary = new Dictionary<Anchor, Vector2>()
        {
            {Anchor.TopLeft, new Vector2(-distance,distance) },
            {Anchor.TopCenter, new Vector2(0,distance) },
            {Anchor.TopRight, new Vector2(distance,distance) },
            {Anchor.CenterLeft, new Vector2(-distance,0) },
            {Anchor.Center, new Vector2(0,0) },
            {Anchor.CenterRight, new Vector2(distance,0) },
            {Anchor.BottomLeft, new Vector2(-distance,-distance) },
            {Anchor.BottomCenter, new Vector2(0,-distance) },
            {Anchor.BottomRight, new Vector2(distance,-distance) }
        };
        public static Dictionary<int, Vector2> solarSystemThreeDictionary = new Dictionary<int, Vector2>()
        {
            {0, GetAnchor(Anchor.TopCenter) },
            {1, GetAnchor(Anchor.BottomLeft) },
            {2, GetAnchor(Anchor.BottomRight)}
        };
        public static Dictionary<int, Vector2> solarSystemFourDictionary = new Dictionary<int, Vector2>()
        {
            {0, GetAnchor(Anchor.TopLeft) },
            {1, GetAnchor(Anchor.TopRight) },
            {2, GetAnchor(Anchor.BottomLeft)},
            {3, GetAnchor(Anchor.BottomRight)}
        };
        public static Dictionary<int, Vector2> solarSystemEightDictionary = new Dictionary<int, Vector2>()
        {
            {0, GetAnchor(Anchor.TopLeft) },
            {1, GetAnchor(Anchor.TopCenter) },
            {2, GetAnchor(Anchor.TopRight) },
            {3, GetAnchor(Anchor.CenterLeft) },
            {4, GetAnchor(Anchor.CenterRight) },
            {5, GetAnchor(Anchor.BottomLeft) },
            {6, GetAnchor(Anchor.BottomCenter) },
            {7, GetAnchor(Anchor.BottomRight) }
        };


        public static Vector2 GetGalaxyOffset(int galaxyIndex)
        {
            Vector2 returnOffset = Vector2.zero;

            if (currentSolarSystemSize == SolarSystemSize.Three)
                returnOffset = solarSystemThreeDictionary[galaxyIndex];
            else if (currentSolarSystemSize == SolarSystemSize.Four)
                returnOffset = solarSystemFourDictionary[galaxyIndex];
            else if (currentSolarSystemSize == SolarSystemSize.Eight)
                returnOffset = solarSystemEightDictionary[galaxyIndex];

            Plugin.DebugLog("Returing GalaxyOffset: " + returnOffset + " SolarSystemSize Is: " + currentSolarSystemSize + " Provided Index Was: " + galaxyIndex);
            return (returnOffset);
        }

        public static Vector2 GetAnchor(Anchor anchor)
        {
            return anchorDictionary[anchor];
        }
    }
}
