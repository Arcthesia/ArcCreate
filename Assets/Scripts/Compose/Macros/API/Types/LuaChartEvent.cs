using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public abstract class LuaChartEvent
    {
        public float Timing { get; set; } = 0;

        public int TimingGroup { get; set; } = 0;

        public bool Attached => Instance != null;

        [MoonSharpHidden]
        public ArcEvent Instance { get; protected set; } = null;

        [EmmyDoc("Create a copy of an event.")]
        public abstract LuaChartEvent Copy();

        [MoonSharpHidden]
        public abstract ArcEvent CreateInstance();

        [EmmyDoc("Create a command that saves changes made to this event.")]
        public LuaChartCommand Save()
        {
            if (Instance == null)
            {
                return new LuaChartCommand(addedEvents: new List<LuaChartEvent>() { this });
            }
            else
            {
                return new LuaChartCommand(editedEvents: new List<LuaChartEvent>() { this });
            }
        }

        [EmmyDoc("Create a command that delete current event, if it's connected to a real event in the chart.")]
        public LuaChartCommand Delete()
        {
            if (Instance == null || (Instance is TimingEvent && Instance.Timing == 0))
            {
                return Command.Create();
            }

            return new LuaChartCommand(removedEvents: new List<LuaChartEvent>() { this });
        }

        [EmmyDoc("Check if the event matches the event type")]
        public bool Is(
#pragma warning disable
            [EmmyChoice("any", "tap", "hold", "arc",
                        "solidarc", "voidarc", "trace", "arctap",
                        "timing", "camera", "floor", "sky",
                        "short", "long", "judgeable")]
            string type)
#pragma warning restore
        {
            var constraint = EventSelectionConstraint.Create().SetType(type);
            return constraint.CheckEvent(this);
        }

        [MoonSharpHidden]
        public void SetInstance(ArcEvent e)
        {
            Instance = e;
        }

        [EmmyDoc("Check if the attached instance equals that of another event")]
        public bool InstanceEquals(LuaChartEvent ev)
        {
            return Instance == ev.Instance;
        }
    }
}