using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    [ExecuteInEditMode]
    public class GroupAnchorLayout : MonoBehaviour
    {
        public RectTransform parentRectTransform;
        public RectTransform rectTransform;
        public RectTransform childRectTransform;
        public int fakeHorizontalCount;
        public int fakeVerticalCount;

        public Vector2 testSize;
        public Vector2 testOffset;

        private void Start() { }

        public void Update()
        {
            if (parentRectTransform == null || rectTransform == null || childRectTransform == null) return;

            float newWidth = parentRectTransform.rect.width / fakeHorizontalCount;
            float newHeight = parentRectTransform.rect.height / fakeVerticalCount;

            rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
            //childRectTransform.sizeDelta = testSize;
            childRectTransform.offsetMin = new Vector2(Mathf.Abs(rectTransform.sizeDelta.x / 2), 0);
            childRectTransform.offsetMax = new Vector2(0, -Mathf.Abs(rectTransform.sizeDelta.y / 2));
        }
    }
}
