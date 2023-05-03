using System.Collections.Generic;
using ArcCreate.Compose.History;
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
        public static LuaChartCommand Create(string name = null)
        {
            return new LuaChartCommand(new List<EventCommand>(), name);
        }
    }
}