using System;
using ArcCreate.Data;
using ArcCreate.Storage.Data;
using UnityEngine;

namespace ArcCreate.Selection.Interface
{
    public static class InterfaceUtility
    {
        public const float HueShift = -0.002777f;
        public const float SatShift = 0.07f;
        public const float ValueShift = -0.15f;

        public static string AlignedDiffNumber(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                return string.Empty;
            }

            char end = number[number.Length - 1];
            if (end == '+' || end == '-')
            {
                return ' ' + number;
            }

            return number;
        }

        public static Color DarkenDiffColor(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            Color rgb = Color.HSVToRGB(h + HueShift, s + SatShift, v + ValueShift);
            rgb.a = color.a;
            return rgb;
        }

        public static bool AreTheSame(LevelStorage a, LevelStorage b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            return a.Id == b.Id;
        }
    }
}