using System.Linq;
using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public class LuaScenecontrol : LuaChartEvent
    {
        public string Type { get; set; }

        public object[] Args { get; set; }

        public override LuaChartEvent Copy()
        {
            return new LuaScenecontrol
            {
                Timing = Timing,
                Type = Type,
                Args = Args,
                TimingGroup = TimingGroup,
            };
        }

        [MoonSharpHidden]
        public override ArcEvent CreateInstance()
        {
            return new ScenecontrolEvent
            {
                Typename = Type,
                Arguments = Args.ToList(),
                Timing = Mathf.RoundToInt(Timing),
                TimingGroup = TimingGroup,
            };
        }
    }
}