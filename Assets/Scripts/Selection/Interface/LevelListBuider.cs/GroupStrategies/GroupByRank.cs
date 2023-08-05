using System.Collections.Generic;
using System.Linq;
using ArcCreate.Data;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;

namespace ArcCreate.Selection.Interface
{
    public class GroupByRank : IGroupStrategy
    {
        public const string Typename = "rank";

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
            string cname = GetName(cells[0].PlayHistory);
            groups.Add((cname, new List<LevelCellData>()));

            foreach (LevelCellData level in cells)
            {
                string name = GetName(level.PlayHistory);
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

        private string GetName(PlayHistory history)
        {
            if (history.PlayCount <= 0)
            {
                return "New";
            }

            ClearResult result = history.BestScorePlayOrDefault.ClearResult;
            switch (result)
            {
                case ClearResult.Fail:
                    return "Fail";
                case ClearResult.Clear:
                    return "Clear";
                case ClearResult.FullCombo:
                    return "Full Combo";
                case ClearResult.AllGood:
                    return "All Good";
                case ClearResult.AllPerfect:
                    return "All Perfect";
                case ClearResult.Max:
                    return "All Perfect+";
                default:
                    return "Unknown";
            }
        }
    }
}