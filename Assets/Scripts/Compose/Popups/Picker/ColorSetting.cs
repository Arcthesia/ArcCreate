using System;
using UnityEngine;

namespace ArcCreate.Compose.Popups
{
    public struct ColorSetting : IEquatable<ColorSetting>
    {
        public int Id;
        public Color Low;
        public Color High;

        public bool Equals(ColorSetting other)
        {
            return Id == other.Id;
        }
    }
}