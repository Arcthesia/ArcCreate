using System.Collections.Generic;
using System.Linq;
using ArcCreate.Utility.InfiniteScroll;
using UnityEngine;

namespace ArcCreate.Compose.Macros
{
    public class MacroPicker : MonoBehaviour
    {
        [SerializeField] private InfiniteScroll scroll;
        [SerializeField] private LastRunMacroTable recentTable;
        private readonly List<MacroDefinition> lastRunMacros = new List<MacroDefinition>();

        public MacroDefinition Selected { get; set; }

        public void SetData(List<MacroDefinition> tree)
        {
            scroll.SetData(tree.Cast<CellData>().ToList());
        }

        public void SetLastRunMacro(MacroDefinition macro)
        {
            for (int i = lastRunMacros.Count - 1; i >= 0; i--)
            {
                if (lastRunMacros[i].Id == macro.Id)
                {
                    lastRunMacros.RemoveAt(i);
                }
            }

            lastRunMacros.Insert(0, macro);
            recentTable.SetData(lastRunMacros);
        }
    }
}