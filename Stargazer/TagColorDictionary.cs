using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    public static class TagColorDictionary
    {
        public static Dictionary<string, Color> tagColorDictionary = new Dictionary<string, Color>()
        {
            {"Wasteland", Color.red},
            {"Valley", Color.green},
            {"Desert", Color.yellow},
            {"Snow", Color.blue},
            {"Company", Color.grey}
        };

        public static bool TryGetTagColor(string[] tags, out Color color)
        {
            color = Color.white;
            foreach (string tag in tags)
                if (TryGetTagColor(tag, out color))
                    return true;
            return false;
        }

        public static bool TryGetTagColor(string tag, out Color color)
        {
            if (tagColorDictionary.TryGetValue(tag, out color))
                return true;
            else
                return false;
        }
    }
}
