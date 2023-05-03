using System.Collections.Generic;
using ArcCreate.Compose.History;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyDoc("An editing command that can be undone / redone")]
    [EmmyGroup("Macros")]
    public class LuaChartCommand
    {
        [MoonSharpHidden]
        public LuaChartCommand(EventCommand command, string name = null)
        {
            Command = new List<EventCommand> { command };
            Name = name;
        }

        [MoonSharpHidden]
        public LuaChartCommand(List<EventCommand> command, string name = null)
        {
            Command = command;
            Name = name;
        }

        [MoonSharpHidden]
        public List<EventCommand> Command { get; set; }

        [EmmyDoc("The command name which is displayed to the user")]
        public string Name { get; set; }

        public static LuaChartCommand operator +(LuaChartCommand c1, LuaChartCommand c2)
        {
            List<EventCommand> command = new List<EventCommand>(c1.Command);
            command.AddRange(c2.Command);
            return new LuaChartCommand(command, c1.Name);
        }

        public static LuaChartCommand operator -(LuaChartCommand c1, LuaChartCommand c2)
        {
            List<EventCommand> command = new List<EventCommand>(c1.Command);
            command.RemoveAll((c) => c2.Command.Contains(c));
            return new LuaChartCommand(command, c1.Name);
        }

        [EmmyDoc("Execute the command.")]
        public void Commit()
        {
            if (Command == null || Command.Count == 0)
            {
                return;
            }

            EventCommand batchCommand = new EventCommand(Name ?? "macro", Command);
            Services.History.AddCommand(batchCommand);
        }

        [EmmyDoc("Combine both command to be executed at once.")]
        public LuaChartCommand Add(LuaChartCommand c)
        {
            Command.AddRange(c.Command);
            return this;
        }
    }
}