using System.Collections.Generic;
using System.Linq;
using ArcCreate.Data;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Animation;
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
        [SerializeField] private GameObject packCellPrefab;
        [SerializeField] private float levelCellSize;
        [SerializeField] private float packCellSize;
        [SerializeField] private float groupCellSize;
        [SerializeField] private float autoScrollDuration = 0.3f;
        private PackStorage currentPack;
        private ChartSettings currentChart;
        private LevelStorage currentLevel;
        private Tween scrollTween;

        private IGroupStrategy groupStrategy;
        private ISortStrategy sortStrategy;

        public static float LevelCellSize { get; set; }

        public static float GroupCellSize { get; set; }

        public static float PackCellSize { get; set; }

        private void Awake()
        {
            Pools.New<Cell>("LevelCell", levelCellPrefab, scroll.transform, 5);
            Pools.New<DifficultyCell>("DifficultyCell", difficultyCellPrefab, scroll.transform, 30);
            Pools.New<Cell>("GroupCell", groupCellPrefab, scroll.transform, 3);

            // Pools.New<Cell>("PackCell", packCellPrefab, transform, 10);
            storageData.OnStorageChange += RebuildList;
            storageData.SelectedChart.OnValueChange += OnSelectedChart;
            storageData.SelectedPack.OnValueChange += OnSelectedPack;

            Settings.SelectionGroupStrategy.OnValueChanged.AddListener(OnGroupStrategyChanged);
            Settings.SelectionSortStrategy.OnValueChanged.AddListener(OnSortStrategyChanged);
            SetGroupStrategy(Settings.SelectionGroupStrategy.Value);
            SetSortStrategy(Settings.SelectionSortStrategy.Value);

            LevelCellSize = levelCellSize;
            GroupCellSize = groupCellSize;
            PackCellSize = packCellSize;
        }

        private void OnDestroy()
        {
            Pools.Destroy<Cell>("LevelCell");
            Pools.Destroy<DifficultyCell>("DifficultyCell");
            Pools.Destroy<Cell>("GroupCell");

            // Pools.Destroy<Cell>("PackCell");
            storageData.OnStorageChange -= RebuildList;
            storageData.SelectedChart.OnValueChange -= OnSelectedChart;
            storageData.SelectedPack.OnValueChange -= OnSelectedPack;

            Settings.SelectionGroupStrategy.OnValueChanged.RemoveListener(OnGroupStrategyChanged);
            Settings.SelectionSortStrategy.OnValueChanged.RemoveListener(OnSortStrategyChanged);
        }

        private void OnSelectedPack(PackStorage pack)
        {
            if (pack != currentPack)
            {
                RebuildList();
            }

            currentPack = pack;
        }

        private void OnSelectedChart((LevelStorage, ChartSettings) obj)
        {
            var (level, chart) = obj;
            if (!chart.IsSameDifficulty(currentChart))
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
                case "none":
                    groupStrategy = new NoGroup();
                    break;
                case "grade":
                    groupStrategy = new GroupByGrade();
                    break;
                case "difficulty":
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
                case "addedDate":
                    sortStrategy = new SortByAddedDate();
                    break;
                case "difficulty":
                    sortStrategy = new SortByDifficulty();
                    break;
                case "grade":
                    sortStrategy = new SortByGrade();
                    break;
                case "score":
                    sortStrategy = new SortByScore();
                    break;
                case "title":
                    sortStrategy = new SortByTitle();
                    break;
                case "composer":
                    sortStrategy = new SortByComposer();
                    break;
                case "charter":
                    sortStrategy = new SortByCharter();
                    break;
                case "playcount":
                    sortStrategy = new SortByPlayCount();
                    break;
                default:
                    sortStrategy = new SortByDifficulty();
                    break;
            }
        }

        private void RebuildList()
        {
            List<LevelStorage> levels = (storageData.SelectedPack.Value?.Levels ?? storageData.GetAllLevels()).ToList();
            List<CellData> data = LevelListBuilder.Build(levels, storageData.SelectedChart.Value.chart, groupStrategy, sortStrategy);
            scroll.SetData(data);

            scrollRect.anchorMin = new Vector2(-0.4f, 0);
            scrollRect.DOAnchorMin(Vector2.zero, autoScrollDuration).SetEase(Ease.OutCubic);

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
    }
}