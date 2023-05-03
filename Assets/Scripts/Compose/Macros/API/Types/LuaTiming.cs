using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public class LuaTiming : LuaChartEvent
    {
        public float Bpm { get; set; }

        public float Divisor { get; set; }

        public override LuaChartEvent Copy()
        {
            return new LuaTiming
            {
                Timing = Timing,
                Bpm = Bpm,
                Divisor = Divisor,
                TimingGroup = TimingGroup,
            };
        }

        [MoonSharpHidden]
        public override ArcEvent CreateInstance()
        {
            return new TimingEvent
            {
                Timing = Mathf.RoundToInt(Timing),
                Bpm = Bpm,
                Divisor = Divisor,
                TimingGroup = TimingGroup,
            };
        }
    }
}