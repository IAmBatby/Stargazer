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
        public RawImage image;

        public Color DefaultColor { get; private set; } = new Color(0.9f, 0.9f, 0.9f, 1f);

        public void Apply(MoonUIVisualizer newFirstVisualizer, MoonUIVisualizer newSecondVisualizer)
        {
            //image.enabled = false;

            firstVisualizer = newFirstVisualizer;
            secondVisualizer = newSecondVisualizer;

            float xDifference = secondVisualizer.rectTransform.localPosition.x - firstVisualizer.rectTransform.localPosition.x;
            float yDifference = secondVisualizer.rectTransform.localPosition.y - firstVisualizer.rectTransform.localPosition.y;



            float angle = Mathf.Atan2(xDifference, yDifference);

            Vector2 position = Vector2.Lerp(firstVisualizer.rectTransform.anchoredPosition + firstVisualizer.moonImage.rectTransform.anchoredPosition, secondVisualizer.rectTransform.anchoredPosition + secondVisualizer.moonImage.rectTransform.anchoredPosition, 0.5f);

            rectTransform.anchoredPosition = position;
            rectTransform.rotation = Quaternion.Euler(0, 0, GetAngle(secondVisualizer.rectTransform.localPosition, firstVisualizer.rectTransform.localPosition));

            float diff = Vector2.Distance(firstVisualizer.rectTransform.anchoredPosition + firstVisualizer.moonImage.rectTransform.anchoredPosition, secondVisualizer.rectTransform.anchoredPosition + secondVisualizer.moonImage.rectTransform.anchoredPosition);
            image = GetComponent<RawImage>();
            image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x, diff / 2.25f);
        }

        public float GetAngle(Vector3 positionA, Vector3 positionB)
        {
            float xDifference = positionB.x - positionA.x;
            float yDifference = positionB.y - positionA.y;

            return (Mathf.Atan2(xDifference, yDifference) * Mathf.Rad2Deg);
        }

        public void RefreshLine()
        {
            if (firstVisualizer.CurrentState != VisualizerState.Active && secondVisualizer.CurrentState != VisualizerState.Active)
                image.color = StarmapUIManager.InactiveColor;
            else
                image.color = DefaultColor;
        }

        private void Update()
        {
            //Vector2 position = Vector2.Lerp(firstVisualizer.rectTransform.anchoredPosition + firstVisualizer.moonImage.rectTransform.anchoredPosition, secondVisualizer.rectTransform.anchoredPosition + secondVisualizer.moonImage.rectTransform.anchoredPosition, 0.5f);

            //rectTransform.anchoredPosition = position;
            //ectTransform.rotation = Quaternion.Euler(0, 0, GetAngle(secondVisualizer.rectTransform.localPosition, firstVisualizer.rectTransform.localPosition));
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
