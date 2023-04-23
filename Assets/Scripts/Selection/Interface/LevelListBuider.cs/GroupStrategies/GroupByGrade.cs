using System.Collections.Generic;
using System.Linq;
using ArcCreate.Data;
using ArcCreate.Utility.InfiniteScroll;

namespace ArcCreate.Selection.Interface
{
    public class GroupByGrade : IGroupStrategy
    {
        public const string Typename = "grade";

        public List<CellData> GroupCells(List<LevelCellData> cells, ISortStrategy sortStrategy)
        {
            if (cells.Count == 0)
            {
                return new List<CellData>();
            }

            List<(string name, List<LevelCellData> cells)> groups = new List<(string, List<LevelCellData>)>();

            cells = cells
                .OrderBy(cell => cell.PlayHistory.BestScorePlayOrDefault.Grade)
                .ThenBy(cell => cell.ChartToDisplay.Title)
                .ToList();

            // Sort to folders
            string cname = GetName(cells[0].PlayHistory.BestScorePlayOrDefault.Grade);
            groups.Add((cname, new List<LevelCellData>()));

            foreach (LevelCellData level in cells)
            {
                string name = GetName(level.PlayHistory.BestScorePlayOrDefault.Grade);
                if (name != cname)
                {
                    cname = name;
                    groups.Add((name, new List<LevelCellData>()));
                }

                groups[groups.Count - 1].cells.Add(level);
            }

            List<CellData> groupCells = new List<CellData>();
            foreach ((string name, List<LevelCellData> group) in groups)
            {
                GroupCellData newGroup = new GroupCellData
                {
                    Pool = Pools.Get<Cell>("GroupCell"),
                    Size = LevelList.GroupCellSize,
                    Children = sortStrategy.Sort(group).ToList<CellData>(),
                    Title = name,
                };
                groupCells.Add(newGroup);
            }

            return groupCells;
        }

        private string GetName(Grade grade)
        {
            switch (grade)
            {
                case Grade.D:
                    return "D";
                case Grade.C:
                    return "C";
                case Grade.B:
                    return "B";
                case Grade.A:
                    return "A";
                case Grade.AA:
                    return "AA";
                case Grade.EX:
                    return "EX";
                case Grade.EXPlus:
                    return "EX+";
                default:
                    return "Unknown";
            }
        }
    }
}