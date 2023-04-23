using System.Collections.Generic;
using System.Linq;

namespace ArcCreate.Selection.Interface
{
    public class SortByScore : ISortStrategy
    {
        public const string Typename = "score";

        public List<LevelCellData> Sort(List<LevelCellData> cells)
        {
            if (cells.Count == 0)
            {
                return cells;
            }

            return cells
                .OrderBy(cell => cell.PlayHistory.BestScorePlayOrDefault.Score)
                .ToList();
        }
    }
}