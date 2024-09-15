using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Stargazer.Components
{
    public class MoonLine : MonoBehaviour
    {
        public MoonUIVisualizer firstVisualizer;
        public MoonUIVisualizer secondVisualizer;

        public RectTransform rectTransform;
        public Image image;

        public void Apply(MoonUIVisualizer newFirstVisualizer, MoonUIVisualizer newSecondVisualizer)
        {
            image.enabled = false;

            firstVisualizer = newFirstVisualizer;
            secondVisualizer = newSecondVisualizer;


            Vector2 position = Vector2.Lerp(firstVisualizer.rectTransform.anchoredPosition + firstVisualizer.moonImage.rectTransform.anchoredPosition, secondVisualizer.rectTransform.anchoredPosition + secondVisualizer.moonImage.rectTransform.anchoredPosition, 0.5f);

            rectTransform.anchoredPosition = position;

            rectTransform.LookAt(secondVisualizer.rectTransform.anchoredPosition + secondVisualizer.moonImage.rectTransform.anchoredPosition);
        }

        public void ApplyLine(Vector3 positionOne, Vector3 positionTwo)
        {
            //m_image.color = color;

            Vector2 point1 = new Vector2(positionTwo.x, positionTwo.y);
            Vector2 point2 = new Vector2(positionOne.x, positionOne.y);
            Vector2 midpoint = (point1 + point2) / 2f;

            rectTransform.position = midpoint;

            Vector2 dir = point1 - point2;
            rectTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            rectTransform.localScale = new Vector3(dir.magnitude, 10f, 1f);
        }
    }
}
