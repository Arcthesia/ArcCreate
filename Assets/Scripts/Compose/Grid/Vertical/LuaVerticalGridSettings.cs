using System;
using System.Collections.Generic;
using ArcCreate.Gameplay;
using ArcCreate.Utility.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    [MoonSharpUserData]
    public class LuaVerticalGridSettings : IVerticalGridSettings, IScriptSetup
    {
        public LuaVerticalGridSettings(string settings, int instructionLimit = 3000)
        {
            try
            {
                UserData.RegisterAssembly();
                LuaRunner.RunScript(settings, this, instructionLimit);
            }
            catch (InstructionLimitReachedException e)
            {
                throw new ComposeException(e.Message);
            }
        }

        [MoonSharpHidden]
        public Rect ColliderArea { get; private set; } = VerticalGrid.DefaultArea;

        [MoonSharpHidden]
        public Color PanelColor { get; private set; } = VerticalGrid.DefaultPanelColor;

        [MoonSharpHidden]
        public List<Line> Lines { get; private set; } = new List<Line>();

        [MoonSharpHidden]
        public List<Area> Areas { get; private set; } = new List<Area>();

        public float SnapTolerance { get; set; } = VerticalGrid.DefaultSnapTolerance;

        [MoonSharpHidden]
        public void SetupScript(Script script)
        {
            script.Globals["grid"] = this;
            script.Globals["Grid"] = this;
            script.Globals["defaultColor"] = new RGBA(VerticalGrid.DefaultLineColor);
            script.Globals["notify"] = (Action<object>)((value) => Services.Popups.Notify(Popups.Severity.Info, value.ToString()));
            script.Globals["notifyWarning"] = (Action<object>)((value) => Services.Popups.Notify(Popups.Severity.Warning, value.ToString()));
            script.Globals["notifyError"] = (Action<object>)((value) => Services.Popups.Notify(Popups.Severity.Error, value.ToString()));
        }

        public void SetCollider(float xFrom, float xTo, float yFrom, float yTo)
        {
            xFrom = ArcFormula.ArcXToWorld(xFrom);
            xTo = ArcFormula.ArcXToWorld(xTo);
            yFrom = ArcFormula.ArcYToWorld(yFrom);
            yTo = ArcFormula.ArcYToWorld(yTo);
            ColliderArea = new Rect(xFrom, yFrom, xTo - xFrom, yTo - yFrom);
        }

        public void SetCollider(XY from, XY to)
        {
            SetCollider(from.X, to.X, from.Y, to.Y);
        }

        public void SetPanelColor(string colorHex)
        {
            if (string.IsNullOrEmpty(colorHex) || !ColorUtility.TryParseHtmlString(colorHex, out Color color))
            {
                color = VerticalGrid.DefaultLineColor;
            }

            PanelColor = color;
        }

        public void SetPanelColor(RGBA rgba)
        {
            PanelColor = rgba.ToColor();
        }

        public void SetPanelColor(HSVA hsva)
        {
            PanelColor = Utility.Lua.Convert.HSVAToRGBA(hsva).ToColor();
        }

        public void DrawLine(float xFrom, float xTo, float yFrom, float yTo)
        {
            DrawLine(xFrom, xTo, yFrom, yTo, VerticalGrid.DefaultLineColor);
        }

        public void DrawLine(float xFrom, float xTo, float yFrom, float yTo, Color color)
        {
            xFrom = ArcFormula.ArcXToWorld(xFrom);
            xTo = ArcFormula.ArcXToWorld(xTo);
            yFrom = ArcFormula.ArcYToWorld(yFrom);
            yTo = ArcFormula.ArcYToWorld(yTo);
            Lines.Add(new Line
            {
                Start = new Vector2(xFrom, yFrom),
                End = new Vector2(xTo, yTo),
                Color = color,
                Interactable = true,
            });
        }

        public void DrawLine(float xFrom, float xTo, float yFrom, float yTo, string colorHex)
        {
            if (string.IsNullOrEmpty(colorHex) || !ColorUtility.TryParseHtmlString(colorHex, out Color color))
            {
                color = VerticalGrid.DefaultLineColor;
            }

            DrawLine(xFrom, xTo, yFrom, yTo, color);
        }

        public void DrawLine(float xFrom, float xTo, float yFrom, float yTo, RGBA color)
        {
            DrawLine(xFrom, xTo, yFrom, yTo, color.ToColor());
        }

        public void DrawLine(float xFrom, float xTo, float yFrom, float yTo, HSVA color)
        {
            DrawLine(xFrom, xTo, yFrom, yTo, Utility.Lua.Convert.HSVAToRGBA(color));
        }

        public void DrawLine(XY from, XY to, string color)
        {
            DrawLine(from.X, to.X, from.Y, to.Y, color);
        }

        public void DrawLine(XY from, XY to, RGBA color)
        {
            DrawLine(from.X, to.X, from.Y, to.Y, color);
        }

        public void DrawLine(XY from, XY to, HSVA color)
        {
            DrawLine(from.X, to.X, from.Y, to.Y, color);
        }

        public void DrawLineDecorative(float xFrom, float xTo, float yFrom, float yTo)
        {
            DrawLineDecorative(xFrom, xTo, yFrom, yTo, VerticalGrid.DefaultLineColor);
        }

        public void DrawLineDecorative(float xFrom, float xTo, float yFrom, float yTo, Color color)
        {
            xFrom = ArcFormula.ArcXToWorld(xFrom);
            xTo = ArcFormula.ArcXToWorld(xTo);
            yFrom = ArcFormula.ArcYToWorld(yFrom);
            yTo = ArcFormula.ArcYToWorld(yTo);
            Lines.Add(new Line
            {
                Start = new Vector2(xFrom, yFrom),
                End = new Vector2(xTo, yTo),
                Color = color,
                Interactable = false,
            });
        }

        public void DrawLineDecorative(float xFrom, float xTo, float yFrom, float yTo, string colorHex)
        {
            if (string.IsNullOrEmpty(colorHex) || !ColorUtility.TryParseHtmlString(colorHex, out Color color))
            {
                color = VerticalGrid.DefaultLineColor;
            }

            DrawLineDecorative(xFrom, xTo, yFrom, yTo, color);
        }

        public void DrawLineDecorative(float xFrom, float xTo, float yFrom, float yTo, RGBA color)
        {
            DrawLineDecorative(xFrom, xTo, yFrom, yTo, color.ToColor());
        }

        public void DrawLineDecorative(float xFrom, float xTo, float yFrom, float yTo, HSVA color)
        {
            DrawLineDecorative(xFrom, xTo, yFrom, yTo, Utility.Lua.Convert.HSVAToRGBA(color));
        }

        public void DrawLineDecorative(XY from, XY to, string color)
        {
            DrawLineDecorative(from.X, to.X, from.Y, to.Y, color);
        }

        public void DrawLineDecorative(XY from, XY to, RGBA color)
        {
            DrawLineDecorative(from.X, to.X, from.Y, to.Y, color);
        }

        public void DrawLineDecorative(XY from, XY to, HSVA color)
        {
            DrawLineDecorative(from.X, to.X, from.Y, to.Y, color);
        }

        public void DrawArea(Color color, params XY[] points)
        {
            if (points.Length < 3)
            {
                return;
            }

            Vector3[] converted = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                XY xy = points[i];
                converted[i] = new Vector3(
                    ArcFormula.ArcXToWorld(xy.X),
                    ArcFormula.ArcYToWorld(xy.Y));
            }

            Area area = new Area()
            {
                Color = color,
                Points = converted,
            };

            Areas.Add(area);
        }

        public void DrawArea(RGBA rgba, params XY[] points)
        {
            DrawArea(rgba.ToColor(), points);
        }

        public void DrawArea(HSVA hsva, params XY[] points)
        {
            DrawArea(Utility.Lua.Convert.HSVAToRGBA(hsva), points);
        }

        public void DrawArea(string colorHex, params XY[] points)
        {
            if (string.IsNullOrEmpty(colorHex) || !ColorUtility.TryParseHtmlString(colorHex, out Color color))
            {
                color = VerticalGrid.DefaultPanelColor;
            }

            DrawArea(color, points);
        }
    }
}