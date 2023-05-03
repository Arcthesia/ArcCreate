using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public class LuaHold : LuaChartEvent
    {
        public int Lane { get; set; } = 1;

        public float EndTiming { get; set; } = 1000;

        public override LuaChartEvent Copy()
        {
            return new LuaHold
            {
                Timing = Timing,
                Lane = Lane,
                EndTiming = EndTiming,
                TimingGroup = TimingGroup,
            };
        }

        [MoonSharpHidden]
        public override ArcEvent CreateInstance()
        {
            return new Hold
            {
                Timing = Mathf.RoundToInt(Timing),
                EndTiming = Mathf.RoundToInt(EndTiming),
                Lane = Lane,
                TimingGroup = TimingGroup,
            };
        }
    }
}