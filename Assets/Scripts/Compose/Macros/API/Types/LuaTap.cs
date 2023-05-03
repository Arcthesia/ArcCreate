using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public class LuaTap : LuaChartEvent
    {
        public int Lane { get; set; } = 1;

        public override LuaChartEvent Copy()
        {
            return new LuaTap
            {
                Timing = Timing,
                Lane = Lane,
                TimingGroup = TimingGroup,
            };
        }

        [MoonSharpHidden]
        public override ArcEvent CreateInstance()
        {
            return new Tap
            {
                Timing = Mathf.RoundToInt(Timing),
                Lane = Lane,
                TimingGroup = TimingGroup,
            };
        }
    }
}