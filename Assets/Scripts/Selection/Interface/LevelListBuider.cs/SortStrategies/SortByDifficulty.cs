using System.Collections.Generic;
using System.Linq;

namespace ArcCreate.Selection.Interface
{
    public class SortByDifficulty : ISortStrategy
    {
        public const string Typename = "difficulty";

        public List<LevelCellData> Sort(List<LevelCellData> cells)
        {
            if (cells.Count == 0)
            {
                return cells;
            }

            return cells
                .OrderBy(cell =>
                {
                    (int diff, bool isPlus) = cell.ChartToDisplay.ParseChartConstant();
                    return isPlus ? diff + 0.1f : diff;
                })
                .ThenBy(cell => cell.ChartToDisplay.Title)
                .ToList();
        }
    }
}