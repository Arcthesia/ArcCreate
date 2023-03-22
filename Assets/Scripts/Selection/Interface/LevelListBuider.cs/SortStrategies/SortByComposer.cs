using System.Collections.Generic;
using System.Linq;

namespace ArcCreate.Selection.Interface
{
    public class SortByComposer : ISortStrategy
    {
        public const string Typename = "Composer";

        public List<LevelCellData> Sort(List<LevelCellData> cells)
        {
            if (cells.Count == 0)
            {
                return cells;
            }

            return cells
                .OrderBy(cell => cell.ChartToDisplay.Composer)
                .ToList();
        }
    }
}