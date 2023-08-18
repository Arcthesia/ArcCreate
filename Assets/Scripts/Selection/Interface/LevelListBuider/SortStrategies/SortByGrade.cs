using System.Collections.Generic;
using System.Linq;

namespace ArcCreate.Selection.Interface
{
    public class SortByGrade : ISortStrategy
    {
        public const string Typename = "grade";

        public List<LevelCellData> Sort(List<LevelCellData> cells)
        {
            if (cells.Count == 0)
            {
                return cells;
            }

            return cells
                .OrderBy(cell => cell.PlayHistory.BestResultPlayOrDefault.ClearResult)
                .ToList();
        }
    }
}