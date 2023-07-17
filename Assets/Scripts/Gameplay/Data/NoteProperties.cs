using System;
using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Skin;
using ArcCreate.Utility;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class NoteProperties
    {
        public Color Color { get; set; } = Color.white;

        public TRS Transform { get; set; } = TRS.identity;

        public Vector2 Angles { get; set; } = Vector2.zero;
    }
}