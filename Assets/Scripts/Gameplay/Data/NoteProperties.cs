using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Skin;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class NoteProperties
    {
        public Color Color { get; set; } = Color.white;

        public Matrix4x4 Matrix { get; set; } = Matrix4x4.identity;

        public Vector2 Angles { get; set; } = Vector2.zero;
    }
}