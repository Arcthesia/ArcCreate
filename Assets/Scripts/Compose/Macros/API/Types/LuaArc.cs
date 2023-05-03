using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Lua;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public class LuaArc : LuaChartEvent
    {
        public XY StartXY { get; set; }

        public XY EndXY { get; set; }

        [MoonSharpHidden]
        public ArcLineType ArcType { get; set; }

        public int Color { get; set; }

        public bool IsTrace { get; set; }

        public float EndTiming { get; set; } = 1000;

        public string Sfx { get; set; } = "none";

        public float StartX => StartXY.X;

        public float StartY => StartXY.Y;

        public float EndX => EndXY.X;

        public float EndY => EndXY.Y;

        public string Type
        {
            get => ArcType.ToLineTypeString();
            set => ArcType = value.ToArcLineType();
        }

        public bool IsVoid { get => IsTrace; set => IsTrace = value; }

        public string Effect { get => Sfx; set => Sfx = value; }

        public override LuaChartEvent Copy()
        {
            return new LuaArc
            {
                Timing = Timing,
                EndTiming = EndTiming,
                StartXY = StartXY,
                EndXY = EndXY,
                ArcType = ArcType,
                Color = Color,
                TimingGroup = TimingGroup,
                IsTrace = IsTrace,
                Sfx = Sfx,
            };
        }

        [MoonSharpHidden]
        public override ArcEvent CreateInstance()
        {
            return new Arc
            {
                Timing = Mathf.RoundToInt(Timing),
                EndTiming = Mathf.RoundToInt(EndTiming),
                XStart = StartX,
                XEnd = EndX,
                LineType = ArcType,
                YStart = StartY,
                YEnd = EndY,
                Color = Color,
                IsTrace = IsTrace,
                TimingGroup = TimingGroup,
                Sfx = Sfx,
            };
        }

        public XY PositionAt(int timing, bool clamp = true)
        {
            float t = (timing - this.Timing) / (EndTiming - this.Timing);
            t = clamp ? Mathf.Clamp(t, 0, 1) : t;
            float x = ArcFormula.X(StartX, EndX, t, ArcType);
            float y = ArcFormula.Y(StartY, EndY, t, ArcType);
            return new XY(x, y);
        }

        public float XAt(int timing, bool clamp = true)
        {
            float t = (timing - this.Timing) / (EndTiming - this.Timing);
            t = clamp ? Mathf.Clamp(t, 0, 1) : t;
            return ArcFormula.X(StartX, EndX, t, ArcType);
        }

        public float YAt(int timing, bool clamp = true)
        {
            float t = (timing - this.Timing) / (EndTiming - this.Timing);
            t = clamp ? Mathf.Clamp(t, 0, 1) : t;
            return ArcFormula.Y(StartY, EndY, t, ArcType);
        }

        [MoonSharpHidden]
        public Arc InstanceAsArc()
        {
            return Instance as Arc;
        }
    }
}