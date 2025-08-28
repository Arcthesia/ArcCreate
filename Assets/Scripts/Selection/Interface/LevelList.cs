using System.Collections.Generic;
using System.Linq;
using ArcCreate.Data;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
            scroll.Value = 0;
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

            //All Songs
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

            // Base Pack
            if (pack.Identifier == "base")
            {
                // Get all levels in the Base pack
                var baseLevels = storageData.GetSinglePackLevels()?.ToList() ?? new List<LevelStorage>();

                // Check if the current level is in the Base pack
                bool found = currentLevel != null && baseLevels.Any(l => l.Id == currentLevel.Id);

                if (!found || currentLevel == null)
                {
                    // Switch to the first level in the Base pack
                    if (baseLevels.Count > 0)
                    {
                        var firstLevel = baseLevels[0];
                        var firstChart = firstLevel.Settings?.Charts?.FirstOrDefault();

                        if (firstChart != null)
                        {
                            currentLevel = firstLevel;
                            storageData.SelectedChart.Value = (firstLevel, firstChart);
                        }
                    }
                }
                else
                {
                    RebuildList();
                }

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
            List<LevelStorage> levels = null;
            if (!lastWasInLevelList) return;

            var prevCount = scroll.Data.Count;

            if (storageData.SelectedPack.Value != null)
            {
                if (storageData.SelectedPack.Value.Identifier != "base")
                {
                    // Normal Pack
                    storageData.FetchLevelsForPack(storageData.SelectedPack.Value);
                    levels = storageData.SelectedPack.Value.Levels?.ToList();
                }
                else
                {
                    // Base Pack
                    levels = storageData.GetSinglePackLevels().ToList();
                }
            }
            else
            {
                // All Songs
                levels = storageData.GetAllLevels()?.ToList();
            }


            levels = levels ?? new List<LevelStorage>();
            Debug.Log(levels?.Count);
            if (levels?.Count == 0)
            {
                packList.BackToPackList();
                return;
            }

            if (string.IsNullOrWhiteSpace(options.SearchQuery))
            {
                var data = LevelListBuilder.Build(
                    levels,
                    storageData.SelectedChart.Value.chart,
                    options.GroupStrategy,
                    options.SortStrategy);
                scroll.SetDataWithoutRebuild(data);
            }
            else
            {
                var data = LevelListBuilder.Filter(levels, storageData.SelectedChart.Value.chart, options.SearchQuery);
                scroll.SetDataWithoutRebuild(data);
            }

            // Only play animation on the second load onward
            if (prevCount > 0)
            {
                scrollRect.anchorMin = new Vector2(-0.4f, 0);
                scrollRect.DOAnchorMin(Vector2.zero, rebuildDuration).SetEase(Ease.OutCubic);
            }

            FocusOnLevelImmediate(currentLevel);
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

        private void FocusOnLevelImmediate(LevelStorage level)
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

            float scrollTo = item.ValueToCenterCell;
            scroll.Value = scrollTo;
            scroll.Rebuild();
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

        private void OnScroll(float arg)
        {
            KillTween();
        }

        private void KillTween()
        {
            scrollTween?.Kill();
        }
    }
}