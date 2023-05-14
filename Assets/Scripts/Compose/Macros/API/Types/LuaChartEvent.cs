using System.Collections.Generic;
using ArcCreate.Compose.History;
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

        public ArcEvent Instance { get; protected set; } = null;

        [EmmyDoc("Create a copy of an event.")]
        public abstract LuaChartEvent Copy();

        [MoonSharpHidden]
        public abstract ArcEvent CreateInstance();

        [EmmyDoc("Create a command that saves changes made to this event.")]
        public LuaChartCommand Save()
        {
            ArcEvent newInstance = CreateInstance();
            EventCommand command;
            if (Instance == null)
            {
                Instance = newInstance;
                command = new EventCommand("Macro", add: new List<ArcEvent> { Instance });
            }
            else
            {
                command = new EventCommand("Macro", update: new List<(ArcEvent instance, ArcEvent newValue)> { (Instance, newInstance) });
            }

            return new LuaChartCommand(command);
        }

        [EmmyDoc("Create a command that delete current event, if it's connected to a real event in the chart.")]
        public LuaChartCommand Delete()
        {
            if (Instance == null || (Instance is TimingEvent && Instance.Timing == 0))
            {
                return Command.Create();
            }

            EventCommand command = new EventCommand("Macro", remove: new List<ArcEvent> { Instance });
            return new LuaChartCommand(command);
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
    }
}