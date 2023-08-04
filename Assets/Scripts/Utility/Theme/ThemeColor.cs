using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Utility
{
    [CreateAssetMenu(fileName = "ThemeColor", menuName = "ScriptableObject/ThemeColor")]
    public class ThemeColor : ScriptableObject
    {
        [SerializeField] private List<ThemeDefinition> colors;
        [SerializeField] private Color fallback = Color.white;

        public Color GetColor(Theme theme)
        {
            for (int i = 0; i < colors.Count; i++)
            {
                ThemeDefinition c = colors[i];
                if (c.Theme == theme)
                {
                    return c.Color;
                }
            }

            return fallback;
        }

        [Serializable]
        public struct ThemeDefinition
        {
            public Theme Theme;
            public Color Color;
        }
    }
}