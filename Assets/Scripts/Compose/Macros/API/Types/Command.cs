using System.Collections.Generic;
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
            List<LuaChartEvent> evAdd = save == null ? null : new List<LuaChartEvent>();
            List<LuaChartEvent> evEdit = save == null ? null : new List<LuaChartEvent>();
            List<LuaChartEvent> evDelete = delete == null ? null : new List<LuaChartEvent>();

            if (save != null)
            {
                foreach (var ev in save)
                {
                    if (ev.Instance == null)
                    {
                        evAdd.Add(ev);
                    }
                    else
                    {
                        evEdit.Add(ev);
                    }
                }
            }

            if (delete != null)
            {
                foreach (var ev in delete)
                {
                    if (ev.Instance != null && !(ev.Instance is TimingEvent && ev.Timing == 0))
                    {
                        evDelete.Add(ev);
                    }
                }
            }

            return new LuaChartCommand(name, addedEvents: evAdd, editedEvents: evEdit, removedEvents: evDelete);
        }
    }
}