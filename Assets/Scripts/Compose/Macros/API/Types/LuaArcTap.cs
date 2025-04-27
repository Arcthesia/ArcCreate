using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public class LuaArcTap : LuaChartEvent
    {
        public LuaArc Arc { get; set; }
        public float Width { get; set; } = 1.0f;

        public int TraceTimingGroup => Arc.TimingGroup;

        public override LuaChartEvent Copy()
        {
            return new LuaArcTap
            {
                Timing = Timing,
                Arc = Arc,
                Width = Width,
            };
        }

        [MoonSharpHidden]
        public override ArcEvent CreateInstance()
        {
            Arc a = Arc.InstanceAsArc();
            ArcTap arctap = new ArcTap
            {
                Timing = Mathf.RoundToInt(Timing),
                Arc = a,
                TimingGroup = TimingGroup,
                Width = Width,
            };

            return arctap;
        }
    }
}