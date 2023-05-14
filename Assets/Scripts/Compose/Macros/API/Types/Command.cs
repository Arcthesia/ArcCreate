using System.Collections.Generic;
using ArcCreate.Compose.History;
using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmySingleton]
    [EmmyGroup("Macros")]
    public class Command
    {
        [EmmyDoc("Create a new command. The provided name will be displayed to the user.")]
        public static LuaChartCommand Create(string name = null, LuaChartEvent[] save = null, LuaChartEvent[] delete = null)
        {
            List<ArcEvent> evAdd = save == null ? null : new List<ArcEvent>();
            List<ArcEvent> evDelete = delete == null ? null : new List<ArcEvent>();
            List<(ArcEvent instance, ArcEvent newValue)> evUpdate = save == null ? null : new List<(ArcEvent instance, ArcEvent newValue)>();

            if (save != null)
            {
                foreach (var ev in save)
                {
                    if (ev.Instance == null)
                    {
                        evAdd.Add(ev.CreateInstance());
                    }
                    else
                    {
                        evUpdate.Add((ev.Instance, ev.CreateInstance()));
                    }
                }
            }

            if (delete != null)
            {
                foreach (var ev in delete)
                {
                    if (ev.Instance != null && !(ev.Instance is TimingEvent && ev.Timing == 0))
                    {
                        evDelete.Add(ev.Instance);
                    }
                }
            }

            EventCommand command = new EventCommand(name, add: evAdd, remove: evDelete, update: evUpdate);
            return new LuaChartCommand(new List<EventCommand>() { command }, name);
        }
    }
}