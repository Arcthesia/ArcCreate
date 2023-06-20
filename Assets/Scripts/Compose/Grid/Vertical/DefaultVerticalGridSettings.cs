using System.Collections.Generic;
using ArcCreate.Gameplay;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    public class DefaultVerticalGridSettings : IVerticalGridSettings
    {
        public Rect ColliderArea => new Rect(-8.5f, 0, 17, 5.5f);

        public Color PanelColor => VerticalGrid.DefaultPanelColor;

        public float SnapTolerance => VerticalGrid.DefaultSnapTolerance;

        public List<Line> Lines
        {
            get
            {
                var lines = new List<Line>();
                for (float x = -0.5f; x <= 1.5f; x += 0.125f)
                {
                    float worldX = ArcFormula.ArcXToWorld(x);
                    lines.Add(new Line()
                    {
                        Start = new Vector2(worldX, 0),
                        End = new Vector2(worldX, 5.5f),
                        Color = VerticalGrid.DefaultLineColor,
                        Interactable = true,
                    });
                }

                for (float y = 0f; y <= 1f; y += 0.25f)
                {
                    float worldY = ArcFormula.ArcYToWorld(y);
                    lines.Add(new Line()
                    {
                        Start = new Vector2(-8.5f, worldY),
                        End = new Vector2(8.5f, worldY),
                        Color = VerticalGrid.DefaultLineColor,
                        Interactable = true,
                    });
                }

                lines.Add(new Line()
                {
                    Start = new Vector2(-8.5f, 0),
                    End = new Vector2(8.5f, 0),
                    Color = VerticalGrid.DefaultLineColor,
                    Interactable = true,
                });
                return lines;
            }
        }

        public List<Area> Areas => new List<Area>();

        public float IncrementX => VerticalGrid.DefaultIncrementX;

        public float IncrementY => VerticalGrid.DefaultIncrementY;
    }
}