using System.Collections.Generic;
using ArcCreate.Data;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;

namespace ArcCreate.Selection.Interface
{
    public class LevelListBuilder
    {
        public static List<CellData> Build(List<LevelStorage> levels, ChartSettings selectedChart, IGroupStrategy groupStrategy, ISortStrategy sortStrategy)
        {
            List<LevelCellData> difficultyMatchLevelCells = new List<LevelCellData>();
            List<LevelCellData> otherLevelCells = new List<LevelCellData>();
            Pool<Cell> pool = Pools.Get<Cell>("LevelCell");

            if (levels.Count == 0)
            {
                return null;
            }

            foreach (LevelStorage level in levels)
            {
                ChartSettings matchingChart = null;
                foreach (ChartSettings chart in level.Settings.Charts)
                {
                    if (chart.IsSameDifficulty(selectedChart))
                    {
                        matchingChart = chart;
                    }
                }

                if (matchingChart != null)
                {
                    PlayHistory history = PlayHistory.GetHistoryForChart(level.Identifier, matchingChart.ChartPath);
                    difficultyMatchLevelCells.Add(new LevelCellData
                    {
                        Pool = pool,
                        ChartToDisplay = matchingChart,
                        LevelStorage = level,
                        PlayHistory = history,
                        Size = LevelList.LevelCellSize,
                    });
                }
                else
                {
                    ChartSettings closestChart = level.Settings.GetClosestDifficultyToChart(selectedChart);
                    PlayHistory history = PlayHistory.GetHistoryForChart(level.Identifier, closestChart.ChartPath);
                    otherLevelCells.Add(new LevelCellData
                    {
                        Pool = pool,
                        ChartToDisplay = closestChart,
                        LevelStorage = level,
                        PlayHistory = history,
                        Size = LevelList.LevelCellSize,
                    });
                }
            }

            List<CellData> result = groupStrategy.GroupCells(difficultyMatchLevelCells, sortStrategy);
            List<CellData> otherDifficultiesGroup = groupStrategy.GroupCells(otherLevelCells, sortStrategy);

            GroupCellData otherDifficultiesFolder = new GroupCellData
            {
                Title = I18n.S("Gameplay.Selection.List.OtherDifficulties"),
                Children = otherDifficultiesGroup,
                Pool = Pools.Get<Cell>("GroupCell"),
                Size = LevelList.GroupCellSize,
            };

            result.Add(otherDifficultiesFolder);
            return result;
        }
    }
}