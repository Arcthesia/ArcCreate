using System.Collections.Generic;
using System.Linq;
using ArcCreate.Data;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;
using UnityEngine;

namespace ArcCreate.Selection.Interface
{
    public class GroupByRating : IGroupStrategy
    {
        public const string Typename = "rating";

        public List<CellData> GroupCells(List<LevelCellData> cells, ISortStrategy sortStrategy)
        {
            if (cells.Count == 0)
            {
                return new List<CellData>();
            }

            List<(string name, List<LevelCellData> cells)> groups = new List<(string, List<LevelCellData>)>();

            cells = cells
                .OrderBy(cell => Mathf.FloorToInt(cell.PlayHistory.Rating))
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
            if (history.Rating <= 0)
            {
                return "N/A";
            }

            return $"★{Mathf.FloorToInt(history.Rating)}";
        }
    }
}