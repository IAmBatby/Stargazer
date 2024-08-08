using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VisualMoonCatalogue
{
    public class VisualMoonGroupContainer : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public List<VisualMoonContainer> visualMoonContainersList = new List<VisualMoonContainer>();
    }
}
