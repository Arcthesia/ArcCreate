using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Data;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;
using FuzzySharp;
using UnityEngine;

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
                    var cellData = new LevelCellData
                    {
                        Pool = pool,
                        ChartToDisplay = matchingChart,
                        LevelStorage = level,
                        PlayHistory = history,
                        Size = LevelList.LevelCellSize,
                    };
                    difficultyMatchLevelCells.Add(cellData);
                }
                else
                {
                    ChartSettings closestChart = level.Settings.GetClosestDifficultyToChart(selectedChart);
                    PlayHistory history = PlayHistory.GetHistoryForChart(level.Identifier, closestChart.ChartPath);
                    var cellData = new LevelCellData
                    {
                        Pool = pool,
                        ChartToDisplay = closestChart,
                        LevelStorage = level,
                        PlayHistory = history,
                        Size = LevelList.LevelCellSize,
                    };
                    otherLevelCells.Add(cellData);
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

        public static List<CellData> Filter(List<LevelStorage> levels, ChartSettings selectedChart, string searchQuery, int threshold = 60)
        {
            Pool<Cell> pool = Pools.Get<Cell>("LevelCell");
            if (levels.Count == 0)
            {
                return null;
            }

            SortedDictionary<int, CellData> filtered = new SortedDictionary<int, CellData>(new ChartScoreComparer());
            searchQuery = searchQuery.ToLower();
            foreach (LevelStorage level in levels)
            {
                int levelCellScore = int.MinValue;
                for (int i = level.Settings.Charts.Count - 1; i >= 0; i--)
                {
                    ChartSettings chart = level.Settings.Charts[i];
                    int score = SearchScore(chart, searchQuery);
                    if (score < threshold)
                    {
                        level.Settings.Charts.RemoveAt(i);
                        continue;
                    }

                    levelCellScore = Math.Max(levelCellScore, score);
                }

                if (levelCellScore > int.MinValue && level.Settings.Charts.Count > 0)
                {
                    ChartSettings matchingChart = null;
                    foreach (ChartSettings chart in level.Settings.Charts)
                    {
                        if (chart.IsSameDifficulty(selectedChart))
                        {
                            matchingChart = chart;
                        }
                    }

                    matchingChart = matchingChart ?? level.Settings.GetClosestDifficultyToChart(selectedChart);

                    PlayHistory history = PlayHistory.GetHistoryForChart(level.Identifier, matchingChart.ChartPath);
                    filtered.Add(levelCellScore, new LevelCellData
                    {
                        Pool = pool,
                        ChartToDisplay = matchingChart,
                        LevelStorage = level,
                        PlayHistory = history,
                        Size = LevelList.LevelCellSize,
                    });
                }
            }

            return filtered.Values.ToList();
        }

        private static int SearchScore(ChartSettings chart, string query)
        {
            return Mathf.Max(
                FieldScore(query, chart.Title),
                FieldScore(query, chart.Composer),
                FieldScore(query, chart.Charter),
                FieldScore(query, chart.SearchTags));
        }

        private static int FieldScore(string query, string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return 0;
            }

            return Fuzz.PartialRatio(query, str.ToLower(), FuzzySharp.PreProcess.PreprocessMode.None);
        }

        public class ChartScoreComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                int result = x.CompareTo(y);

                return result == 0 ? -1 : -result;
            }
        }
    }
}