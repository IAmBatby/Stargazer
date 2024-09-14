using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    public class VisualMoonGroupContainer : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public List<VisualMoonContainer> visualMoonContainersList = new List<VisualMoonContainer>();
    }
}
