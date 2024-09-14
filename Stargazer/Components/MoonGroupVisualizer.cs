using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    public class MoonGroupVisualizer : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public List<MoonVisualizer> visualMoonContainersList = new List<MoonVisualizer>();
    }
}
