using System.Collections.Generic;
using System.Linq;

namespace ArcCreate.Selection.Interface
{
    public class SortByCharter : ISortStrategy
    {
        public const string Typename = "charter";

        public List<LevelCellData> Sort(List<LevelCellData> cells)
        {
            if (cells.Count == 0)
            {
                return cells;
            }

            return cells
                .OrderBy(cell => cell.LevelStorage.Identifier)
                .ToList();
        }
    }
}