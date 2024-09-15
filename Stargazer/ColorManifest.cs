using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    [CreateAssetMenu(fileName = "ColorManifest", menuName = "Stargazer/ColorManifest", order = 1)]
    public class ColorManifest : ScriptableObject
    {
        public List<TagWithColor> tags = new List<TagWithColor>();

        public Dictionary<string, Color> CreateDictionary()
        {
            Dictionary<string, Color> returnDict = new Dictionary<string, Color>();

            foreach (TagWithColor tag in tags)
                if (!returnDict.ContainsKey(tag.tag))
                    returnDict.Add(tag.tag, tag.color);

            return (returnDict);
        }
    }


    [System.Serializable]
    public class TagWithColor
    {
        public string tag;
        public Color color;
    }
}
