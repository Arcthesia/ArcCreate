using System;
using System.Collections.Generic;
using System.Linq;
using ArcCreate.Data;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class LevelList : MonoBehaviour
    {
        private static bool lastWasInLevelList = false;

        [SerializeField] private StorageData storageData;
        [SerializeField] private InfiniteScroll scroll;
        [SerializeField] private LevelListOptions options;
        [SerializeField] private RectTransform scrollRect;
        [SerializeField] private GameObject levelCellPrefab;
        [SerializeField] private GameObject difficultyCellPrefab;
        [SerializeField] private GameObject groupCellPrefab;
        [SerializeField] private float levelCellSize;
        [SerializeField] private float groupCellSize;
        [SerializeField] private float autoScrollDuration = 1f;
        [SerializeField] private float rebuildDuration = 0.3f;
        [SerializeField] private PackList packList;
        [SerializeField] private Button randomButton;
        [SerializeField] private Button jumpToTopButton;
        [SerializeField] private Button jumpToBottomButton;
        private PackStorage currentPack;
        private ChartSettings currentChart;
        private LevelStorage currentLevel;
        private Tween scrollTween;

        public static float LevelCellSize { get; set; }

        public static float GroupCellSize { get; set; }

        private void Awake()
        {
            options.Setup();
            Pools.New<Cell>("LevelCell", levelCellPrefab, scroll.transform, 5);
            Pools.New<DifficultyCell>("DifficultyCell", difficultyCellPrefab, scroll.transform, 30);
            Pools.New<Cell>("GroupCell", groupCellPrefab, scroll.transform, 3);

            storageData.OnStorageChange += OnStorageChange;
            storageData.SelectedChart.OnValueChange += OnSelectedChart;
            storageData.SelectedPack.OnValueChange += OnSelectedPack;
            options.OnNeedRebuild += RebuildList;

            randomButton.onClick.AddListener(SelectRandom);
            jumpToTopButton.onClick.AddListener(SelectTop);
            jumpToBottomButton.onClick.AddListener(SelectBottom);

            LevelCellSize = levelCellSize;
            GroupCellSize = groupCellSize;

            scroll.OnPointerEvent += KillTween;

            if (storageData.IsLoaded && lastWasInLevelList)
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
            options.OnNeedRebuild -= RebuildList;

            randomButton.onClick.RemoveListener(SelectRandom);
            jumpToTopButton.onClick.RemoveListener(SelectTop);
            jumpToBottomButton.onClick.RemoveListener(SelectBottom);

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
            lastWasInLevelList = true;
            if (pack == null)
            {
                var (level, chart) = storageData.GetLastSelectedChart(null);
                if (level != null && chart != null)
                {
                    currentLevel = level;
                    storageData.SelectedChart.Value = (level, chart);
                }

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

            currentPack = pack;
        }

        private void OnSelectedChart((LevelStorage, ChartSettings) obj)
        {
            var (level, chart) = obj;
            bool chartChanged = !chart.IsSameDifficulty(currentChart, false);
            bool packChanged = storageData.SelectedPack.Value != currentPack;
            if (chartChanged || packChanged)
            {
                RebuildList();
            }

            FocusOnLevel(level);
            currentChart = chart;
            currentLevel = level;
        }

        private void RebuildList()
        {
            if (!lastWasInLevelList)
            {
                return;
            }

            int prevCount = scroll.Data.Count;
            if (storageData.SelectedPack.Value != null)
            {
                storageData.FetchLevelsForPack(storageData.SelectedPack.Value);
            }

            List<LevelStorage> levels = (storageData.SelectedPack.Value?.Levels ?? storageData.GetAllLevels())?.ToList();
            if (levels?.Count == 0)
            {
                packList.BackToPackList();
                return;
            }

            if (string.IsNullOrWhiteSpace(options.SearchQuery))
            {
                List<CellData> data = LevelListBuilder.Build(
                    levels,
                    storageData.SelectedChart.Value.chart,
                    options.GroupStrategy,
                    options.SortStrategy);
                scroll.SetData(data);
            }
            else
            {
                List<CellData> data = LevelListBuilder.Filter(levels, storageData.SelectedChart.Value.chart, options.SearchQuery);
                scroll.SetData(data);
            }

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

            if (item == null)
            {
                return;
            }

            float scrollFrom = scroll.Value;
            float scrollTo = item.ValueToCenterCell;
            KillTween();
            scrollTween = DOTween.To((float val) => scroll.Value = val, scrollFrom, scrollTo, autoScrollDuration).SetEase(Ease.OutExpo);
        }

        private void SelectRandom()
        {
            List<LevelStorage> levels = (storageData.SelectedPack.Value?.Levels ?? storageData.GetAllLevels())?.ToList();
            if (levels.Count <= 0)
            {
                return;
            }

            LevelStorage level = null;

            do
            {
                int index = UnityEngine.Random.Range(0, levels.Count);
                level = levels[index];
            }
            while (InterfaceUtility.AreTheSame(level, storageData.SelectedChart.Value.level));

            LevelCellData item = null;
            for (int i = 0; i < scroll.Data.Count; i++)
            {
                CellData cell = scroll.Data[i];
                if (cell is LevelCellData lvCell && InterfaceUtility.AreTheSame(lvCell.LevelStorage, level))
                {
                    item = lvCell;
                    break;
                }
            }

            if (item == null)
            {
                return;
            }

            storageData.SelectedChart.Value = (item.LevelStorage, item.ChartToDisplay);
        }

        private void SelectTop()
        {
            scrollTween = DOTween.To((float val) => scroll.Value = val, scroll.Value, 0, autoScrollDuration / 2).SetEase(Ease.OutExpo);
        }

        private void SelectBottom()
        {
            float v = 0;
            if (scroll.Hierarchy.Count >= 1)
            {
                v = scroll.Hierarchy[scroll.Hierarchy.Count - 1].ValueToCenterCell;
            }

            scrollTween = DOTween.To((float val) => scroll.Value = val, scroll.Value, v, autoScrollDuration / 2).SetEase(Ease.OutExpo);
        }

        private void KillTween()
        {
            scrollTween?.Kill();
        }
    }
}