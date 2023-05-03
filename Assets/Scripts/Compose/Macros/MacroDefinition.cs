using System;
using System.Collections.Generic;
using ArcCreate.Utility.InfiniteScroll;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    public class MacroDefinition : CellData, IComparable<MacroDefinition>
    {
        private static readonly HashSet<string> ExpandedIds = new HashSet<string>();

        public MacroDefinition(string id)
        {
            Id = id;
            CollapsedByDefault = !ExpandedIds.Contains(id);
        }

        public string Icon { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public Script Script { get; set; }

        public DynValue Callback { get; set; }

        public static void ToggleCollapseByDefault(string macroId)
        {
            if (ExpandedIds.Contains(macroId))
            {
                ExpandedIds.Remove(macroId);
            }
            else
            {
                ExpandedIds.Add(macroId);
            }
        }

        public int CompareTo(MacroDefinition other)
        {
            return Id.CompareTo(other.Id);
        }

        public void RemoveNode(string id)
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                CellData child = Children[i];
                if (child is MacroDefinition macro)
                {
                    if (macro.Id == id)
                    {
                        Children.RemoveAt(i);
                    }

                    macro.RemoveNode(id);
                }
            }
        }
    }
}