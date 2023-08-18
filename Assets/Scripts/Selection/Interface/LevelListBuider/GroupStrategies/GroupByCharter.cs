using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Utility.InfiniteScroll;

namespace ArcCreate.Selection.Interface
{
    public class GroupByCharter : IGroupStrategy
    {
        public const string Typename = "charter";

        public List<CellData> GroupCells(List<LevelCellData> cells, ISortStrategy sortStrategy)
        {
            if (cells.Count == 0)
            {
                return new List<CellData>();
            }

            List<(string name, List<LevelCellData> cells)> groups = new List<(string, List<LevelCellData>)>();

            cells = cells
                .OrderBy(cell => cell.LevelStorage.Identifier)
                .ThenBy(cell => cell.ChartToDisplay.Title)
                .ToList();

            // Sort to folders
            string cname = GetCharterName(cells[0].LevelStorage.Identifier);
            groups.Add((cname, new List<LevelCellData>()));

            foreach (LevelCellData level in cells)
            {
                string name = GetCharterName(level.LevelStorage.Identifier);
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

        private string GetCharterName(string identifier)
        {
            int index = identifier.IndexOf('.');
            if (index >= 0 && index < identifier.Length)
            {
                return identifier.Substring(0, index);
            }

            return "Unknown";
        }
    }
}