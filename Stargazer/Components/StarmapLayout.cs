using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    public class StarmapLayout : MonoBehaviour
    {
        public List<StarmapLayoutRow> starmapColumns = new List<StarmapLayoutRow>();

        public MoonGroupUIVisualizer specialGroup;
    }

    [Serializable]
    public class StarmapLayoutRow
    {
        public List<MoonGroupUIVisualizer> allVisualizers = new List<MoonGroupUIVisualizer>();
    }
}
