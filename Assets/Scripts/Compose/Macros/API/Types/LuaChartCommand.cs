using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.History;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyDoc("An editing command that can be undone / redone")]
    [EmmyGroup("Macros")]
    public class LuaChartCommand
    {
        private List<LuaChartEvent> addedEvents;
        private List<LuaChartEvent> removedEvents;
        private List<LuaChartEvent> editedEvents;
        private List<LuaTimingGroup> addedTG;
        private List<LuaTimingGroup> removedTG;
        private List<LuaTimingGroup> editedTG;
        private readonly Dictionary<LuaTimingGroup, List<LuaChartEvent>> groupAssignment
            = new Dictionary<LuaTimingGroup, List<LuaChartEvent>>();

        [MoonSharpHidden]
        public LuaChartCommand(string name = null,
            List<LuaChartEvent> addedEvents = null,
            List<LuaChartEvent> removedEvents = null,
            List<LuaChartEvent> editedEvents = null,
            List<LuaTimingGroup> addedTG = null,
            List<LuaTimingGroup> removedTG = null,
            List<LuaTimingGroup> editedTG = null
            )
        {
            Name = name;
            this.addedEvents = addedEvents ?? new List<LuaChartEvent>();
            this.removedEvents = removedEvents ?? new List<LuaChartEvent>();
            this.editedEvents = editedEvents ?? new List<LuaChartEvent>();
            this.addedTG = addedTG ?? new List<LuaTimingGroup>();
            this.removedTG = removedTG ?? new List<LuaTimingGroup>();
            this.editedTG = editedTG ?? new List<LuaTimingGroup>();
        }

        [EmmyDoc("The command name which is displayed to the user")]
        public string Name { get; set; }

        [EmmyDoc("Combine both command to be executed at once.")]
        public LuaChartCommand Add(LuaChartCommand c)
        {
            addedEvents.AddRange(c.addedEvents);
            removedEvents.AddRange(c.removedEvents);
            editedEvents.AddRange(c.editedEvents);
            addedTG.AddRange(c.addedTG);
            removedTG.AddRange(c.removedTG);
            editedTG.AddRange(c.editedTG);
            foreach (var pair in c.groupAssignment)
            {
                if (groupAssignment.ContainsKey(pair.Key))
                {
                    groupAssignment[pair.Key].AddRange(pair.Value);
                }
                else
                {
                    groupAssignment.Add(pair.Key, pair.Value);
                }
            }

            return this;
        }
        
        [EmmyDoc("Assign all events under this command to a timing group on command execution. Useful for assign to newly created timing group.")]
        public LuaChartCommand WithTimingGroup(LuaTimingGroup timingGroup)
        {
            if (groupAssignment.ContainsKey(timingGroup))
            {
                groupAssignment[timingGroup].AddRange(addedEvents);
                groupAssignment[timingGroup].AddRange(editedEvents);
            }
            else
            {
                List<LuaChartEvent> evs = new List<LuaChartEvent>();
                evs.AddRange(addedEvents);
                evs.AddRange(editedEvents);
                groupAssignment.Add(timingGroup, evs);
            }

            return this;
        }

        [EmmyDoc("Execute the command.")]
        public void Commit()
        {
            List<ICommand> commands = new List<ICommand>();
            foreach (var remove in removedTG)
            {
                if (remove.Instance != null)
                {
                    ICommand cmd = new RemoveTimingGroupCommand("Macro", remove.Instance);
                    cmd.Execute();
                    commands.Add(cmd);
                    remove.Instance = null;
                }
            }

            foreach (var edit in editedTG)
            {
                if (edit.Instance != null)
                {
                    GroupProperties newProps = new GroupProperties(edit.ToProperty());
                    ICommand cmd = new EditTimingGroupProperitesCommand("Macro", edit.Instance, newProps);
                    cmd.Execute();
                    commands.Add(cmd);
                    edit.SetProperties(edit.Instance.GroupProperties.ToRaw());
                }
            }

            foreach (var add in addedTG)
            {
                if (add.Instance == null)
                {
                    int newTgNum = Services.Gameplay.Chart.TimingGroups.Count;
                    TimingGroup group = new TimingGroup(newTgNum);
                    group.Load();
                    group.SetProperties(add.ToProperty());
                    ICommand cmd = new AddTimingGroupCommand("Macro", group);
                    cmd.Execute();
                    commands.Add(cmd);
                    add.Instance = group;
                }
            }

            foreach (var pair in groupAssignment)
            {
                foreach (var ev in pair.Value)
                {
                    ev.TimingGroup = pair.Key.Instance.GroupNumber;
                }
            }

            HashSet<ArcEvent> removed = new HashSet<ArcEvent>();
            HashSet<ArcEvent> added = new HashSet<ArcEvent>();
            Dictionary<ArcEvent, ArcEvent> edited = new Dictionary<ArcEvent, ArcEvent>();
            foreach (var remove in removedEvents)
            {
                if (remove.Instance != null)
                {
                    removed.Add(remove.Instance);
                    remove.SetInstance(null);
                }
            }

            foreach (var add in addedEvents)
            {
                if (add.Instance == null)
                {
                    ArcEvent newvalue = add.CreateInstance();
                    added.Add(newvalue);
                    add.SetInstance(newvalue);
                }
            }

            foreach (var edit in editedEvents)
            {
                if (edit.Instance != null)
                {
                    ArcEvent newvalue = edit.CreateInstance();
                    if (edited.ContainsKey(edit.Instance))
                    {
                        edited[edit.Instance] = newvalue;
                    }
                    else
                    {
                        edited.Add(edit.Instance, newvalue);
                    }
                }
            }

            HashSet<(ArcEvent, ArcEvent)> updated = edited.Select(pair => (pair.Key, pair.Value)).ToHashSet();
            EventCommand evcmd = new EventCommand("Macro", add: added, remove: removed, update: updated);
            evcmd.Execute();
            commands.Add(evcmd);

            ICommand batchCommand = new CombinedCommand(Name ?? "macro", commands.ToArray());
            Services.History.AddCommandWithoutExecuting(batchCommand);
        }
    }
}