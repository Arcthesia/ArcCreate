using System.Collections.Generic;
using System.Linq;
using ArcCreate.Utility.InfiniteScroll;

namespace ArcCreate.Selection.Interface
{
    public class GroupByDifficulty : IGroupStrategy
    {
        public const string Typename = "Difficulty";

        public List<CellData> GroupCells(List<LevelCellData> cells, ISortStrategy sortStrategy)
        {
            if (cells.Count == 0)
            {
                return new List<CellData>();
            }

            List<(int diff, bool isPlus, List<LevelCellData> cells)> groups = new List<(int, bool, List<LevelCellData>)>();

            cells = cells
                .OrderBy(cell =>
                {
                    (int diff, bool isPlus) = cell.ChartToDisplay.ParseChartConstant();
                    return isPlus ? diff + 0.1f : diff;
                })
                .ThenBy(cell => cell.ChartToDisplay.Title)
                .ToList();

            // Sort to folders
            (int cdiff, bool cisPlus) = cells[0].ChartToDisplay.ParseChartConstant();
            groups.Add((cdiff, cisPlus, new List<LevelCellData>()));

            foreach (LevelCellData level in cells)
            {
                (int diff, bool isPlus) = level.ChartToDisplay.ParseChartConstant();
                if (diff != cdiff || cisPlus != isPlus)
                {
                    cdiff = diff;
                    cisPlus = isPlus;
                    groups.Add((diff, isPlus, new List<LevelCellData>()));
                }

                groups[groups.Count - 1].cells.Add(level);
            }

            List<CellData> groupCells = new List<CellData>();
            foreach ((int diff, bool isPlus, List<LevelCellData> group) in groups)
            {
                GroupCellData newGroup = new GroupCellData
                {
                    Pool = Pools.Get<Cell>("GroupCell"),
                    Size = LevelList.GroupCellSize,
                    Children = sortStrategy.Sort(group).ToList<CellData>(),
                    Title = $"LEVEL {diff}{(isPlus ? "+" : "")}",
                };
                groupCells.Add(newGroup);
            }

            return groupCells;
        }
    }
}