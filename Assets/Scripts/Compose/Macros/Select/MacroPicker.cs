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
            lastRunMacros.Remove(macro);
            lastRunMacros.Insert(0, macro);
            recentTable.SetData(lastRunMacros);
        }
    }
}