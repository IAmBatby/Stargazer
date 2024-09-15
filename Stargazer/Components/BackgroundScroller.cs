using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer
{
    public class BackgroundScroller : MonoBehaviour
    {
        public RawImage backgroundImage;
        public Vector2 scrollSpeed;

        private void Update()
        {
            backgroundImage.uvRect = new Rect(backgroundImage.uvRect.position + scrollSpeed * Time.deltaTime, backgroundImage.uvRect.size);
        }
    }
}
