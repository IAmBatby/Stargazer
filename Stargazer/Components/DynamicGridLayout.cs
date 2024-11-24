using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer
{
    [ExecuteAlways]
    public class DynamicGridLayout : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;


        private int previousCount = -1;

        public void Update()
        {
            if (previousCount == -1)
                previousCount = gridLayoutGroup.constraintCount;

            if (gridLayoutGroup.constraintCount != previousCount)
                RefreshDynamicGridLayout(gridLayoutGroup.constraintCount);
        }

        public void RefreshDynamicGridLayout(int newRowCount)
        {
            gridLayoutGroup.constraintCount = newRowCount;
            //gridLayoutGroup.cellSize = (rectTransform.rect.size - (gridLayoutGroup.spacing * newRowCount)) / newRowCount;
            gridLayoutGroup.cellSize = rectTransform.rect.size / newRowCount;
            previousCount = newRowCount;
        }

        public void RefreshLayout()
        {
            gridLayoutGroup.enabled = false;
            gridLayoutGroup.enabled = true;
        }
    }
}
