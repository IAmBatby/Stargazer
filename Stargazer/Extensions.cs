using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Stargazer
{
    internal static class Extensions
    {
        internal static float Lerp(this float currentValue, float target, float t) => (Mathf.Lerp(currentValue, target, 1 - Mathf.Pow(t, Time.deltaTime)));
        internal static Vector2 Lerp(this Vector2 currentValue, Vector2 target, float t) => new Vector2(Lerp(currentValue.x, target.x, t), Lerp(currentValue.y, target.y, t));
        internal static Vector3 Lerp(this Vector3 currentValue, Vector3 target, float t) => new Vector3(Lerp(currentValue.x, target.x, t), Lerp(currentValue.y, target.y, t), Lerp(currentValue.z, target.z, t));
    }
}
