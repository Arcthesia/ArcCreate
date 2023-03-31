using System.Collections.Generic;
using System.Linq;
using ArcCreate.Data;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;
using DG.Tweening;
using UnityEngine;

namespace ArcCreate.Selection.Interface
{
    public class LevelList : MonoBehaviour
    {
        [SerializeField] private StorageData storageData;
        [SerializeField] private InfiniteScroll scroll;
        [SerializeField] private RectTransform scrollRect;
        [SerializeField] private GameObject levelCellPrefab;
        [SerializeField] private GameObject difficultyCellPrefab;
        [SerializeField] private GameObject groupCellPrefab;
        [SerializeField] private float levelCellSize;
        [SerializeField] private float groupCellSize;
        [SerializeField] private float autoScrollDuration = 1f;
        [SerializeField] private float rebuildDuration = 0.3f;
        [SerializeField] private PackList packList;
        private PackStorage currentPack;
        private ChartSettings currentChart;
        private LevelStorage currentLevel;
        private Tween scrollTween;

        private IGroupStrategy groupStrategy;
        private ISortStrategy sortStrategy;

        public static float LevelCellSize { get; set; }

        public static float GroupCellSize { get; set; }

        private void Awake()
        {
            Pools.New<Cell>("LevelCell", levelCellPrefab, scroll.transform, 5);
            Pools.New<DifficultyCell>("DifficultyCell", difficultyCellPrefab, scroll.transform, 30);
            Pools.New<Cell>("GroupCell", groupCellPrefab, scroll.transform, 3);

            storageData.OnStorageChange += OnStorageChange;
            storageData.SelectedChart.OnValueChange += OnSelectedChart;
            storageData.SelectedPack.OnValueChange += OnSelectedPack;

            Settings.SelectionGroupStrategy.OnValueChanged.AddListener(OnGroupStrategyChanged);
            Settings.SelectionSortStrategy.OnValueChanged.AddListener(OnSortStrategyChanged);
            SetGroupStrategy(Settings.SelectionGroupStrategy.Value);
            SetSortStrategy(Settings.SelectionSortStrategy.Value);

            LevelCellSize = levelCellSize;
            GroupCellSize = groupCellSize;

            scroll.OnPointerEvent += KillTween;

            if (storageData.IsLoaded)
            {
                OnStorageChange();
            }
        }

        private void OnDestroy()
        {
            Pools.Destroy<Cell>("LevelCell");
            Pools.Destroy<DifficultyCell>("DifficultyCell");
            Pools.Destroy<Cell>("GroupCell");

            storageData.OnStorageChange -= OnStorageChange;
            storageData.SelectedChart.OnValueChange -= OnSelectedChart;
            storageData.SelectedPack.OnValueChange -= OnSelectedPack;

            Settings.SelectionGroupStrategy.OnValueChanged.RemoveListener(OnGroupStrategyChanged);
            Settings.SelectionSortStrategy.OnValueChanged.RemoveListener(OnSortStrategyChanged);

            scroll.OnPointerEvent -= KillTween;
        }

        private void OnStorageChange()
        {
            currentPack = storageData.SelectedPack.Value;
            (currentLevel, currentChart) = storageData.SelectedChart.Value;
            RebuildList();
        }

        private void OnSelectedPack(PackStorage pack)
        {
            if (pack != currentPack)
            {
                if (pack == null)
                {
                    RebuildList();
                    currentPack = pack;
                    return;
                }

                bool found = false;
                foreach (var level in pack.Levels)
                {
                    if (currentLevel != null && level.Id == currentLevel.Id)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    var (level, chart) = storageData.GetLastSelectedChart(pack?.Identifier);
                    if (level != null && chart != null)
                    {
                        currentLevel = level;
                        storageData.SelectedChart.Value = (level, chart);
                    }
                }
                else
                {
                    RebuildList();
                }
            }

            currentPack = pack;
        }

        private void OnSelectedChart((LevelStorage, ChartSettings) obj)
        {
            var (level, chart) = obj;
            if (!chart.IsSameDifficulty(currentChart) || storageData.SelectedPack.Value != currentPack)
            {
                RebuildList();
            }

            FocusOnLevel(level);
            currentChart = chart;
            currentLevel = level;
        }

        private void OnGroupStrategyChanged(string strat)
        {
            SetGroupStrategy(strat);
            RebuildList();
        }

        private void SetGroupStrategy(string strat)
        {
            switch (strat)
            {
                case NoGroup.Typename:
                    groupStrategy = new NoGroup();
                    break;
                case GroupByGrade.Typename:
                    groupStrategy = new GroupByGrade();
                    break;
                case GroupByDifficulty.Typename:
                    groupStrategy = new GroupByDifficulty();
                    break;
                default:
                    groupStrategy = new NoGroup();
                    break;
            }
        }

        private void OnSortStrategyChanged(string strat)
        {
            SetSortStrategy(strat);
            RebuildList();
        }

        private void SetSortStrategy(string strat)
        {
            switch (strat)
            {
                case SortByAddedDate.Typename:
                    sortStrategy = new SortByAddedDate();
                    break;
                case SortByDifficulty.Typename:
                    sortStrategy = new SortByDifficulty();
                    break;
                case SortByGrade.Typename:
                    sortStrategy = new SortByGrade();
                    break;
                case SortByScore.Typename:
                    sortStrategy = new SortByScore();
                    break;
                case SortByTitle.Typename:
                    sortStrategy = new SortByTitle();
                    break;
                case SortByComposer.Typename:
                    sortStrategy = new SortByComposer();
                    break;
                case SortByCharter.Typename:
                    sortStrategy = new SortByCharter();
                    break;
                case SortByPlayCount.Typename:
                    sortStrategy = new SortByPlayCount();
                    break;
                default:
                    sortStrategy = new SortByDifficulty();
                    break;
            }
        }

        private void RebuildList()
        {
            int prevCount = scroll.Data.Count;
            List<LevelStorage> levels = (storageData.SelectedPack.Value?.Levels ?? storageData.GetAllLevels())?.ToList();
            if (levels?.Count == 0)
            {
                packList.BackToPackList();
                return;
            }

            List<CellData> data = LevelListBuilder.Build(levels, storageData.SelectedChart.Value.chart, groupStrategy, sortStrategy);
            scroll.SetData(data);

            // Only play animation on the second load onward
            if (prevCount > 0)
            {
                scrollRect.anchorMin = new Vector2(-0.4f, 0);
                scrollRect.DOAnchorMin(Vector2.zero, rebuildDuration).SetEase(Ease.OutCubic);
            }

            FocusOnLevel(currentLevel);
        }

        private void FocusOnLevel(LevelStorage level)
        {
            HierarchyData item = null;
            if (level == null)
            {
                return;
            }

            for (int i = 0; i < scroll.Data.Count; i++)
            {
                CellData data = scroll.Data[i];
                if (data is LevelCellData levelCell && levelCell.LevelStorage.Id == level.Id)
                {
                    item = scroll.Hierarchy[i];
                    break;
                }
            }

            float scrollFrom = scroll.Value;
            float scrollTo = item.ValueToCenterCell;
            scrollTween?.Kill();
            scrollTween = DOTween.To((float val) => scroll.Value = val, scrollFrom, scrollTo, autoScrollDuration).SetEase(Ease.OutExpo);
        }

        private void KillTween()
        {
            scrollTween?.Kill();
        }
    }
}