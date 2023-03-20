using System.Collections.Generic;
using System.Threading;
using ArcCreate.Data;
using ArcCreate.Selection.Select;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class LevelCell : Cell, IPointerClickHandler
    {
        private static Pool<DifficultyCell> difficultyCellPool;
        [SerializeField] private StorageData storage;
        [SerializeField] private SelectableStorage selectable;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text composer;
        [SerializeField] private TMP_Text difficulty;
        [SerializeField] private RawImage jacket;
        [SerializeField] private DifficultyCell difficultyBackground;
        [SerializeField] private Transform difficultyCellParent;
        [SerializeField] private GameObject playIndicate;

        [Header("Animation")]
        [SerializeField] private RectTransform rect;
        [SerializeField] private Vector2 selectedSizeDelta;
        [SerializeField] private float animationDuration = 0.15f;
        [SerializeField] private Ease animationEase = Ease.OutExpo;

        private LevelStorage level;
        private readonly List<DifficultyCell> difficultyCells = new List<DifficultyCell>();
        private ChartSettings visibleChart;
        private Vector2 defaultSizeDelta;
        private bool isSelected;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Services.Select.IsAnySelected || storage.IsTransitioning)
            {
                return;
            }

            if (isSelected)
            {
                storage.SwitchToPlayScene((level, visibleChart));
            }
            else
            {
                storage.SelectedChart.Value = (level, visibleChart);
            }
        }

        public override void SetCellData(CellData cellData)
        {
            LevelCellData data = cellData as LevelCellData;
            level = data.LevelStorage;
            selectable.StorageUnit = level;
            visibleChart = data.ChartToDisplay;

            UpdateSelectedStateImmediate(InterfaceUtility.AreTheSame(level, storage.SelectedChart.Value.level));
            difficultyCellPool = difficultyCellPool ?? Pools.Get<DifficultyCell>("DifficultyCell");
            SetInfo();
        }

        public override async UniTask LoadCellFully(CellData cellData, CancellationToken cancellationToken)
        {
            await storage.AssignTexture(jacket, level, visibleChart.JacketPath);
        }

        private void SetInfo()
        {
            title.text = visibleChart.Title;
            composer.text = visibleChart.Composer;
            ColorUtility.TryParseHtmlString(visibleChart.DifficultyColor, out Color color);
            difficultyBackground.Color = color;

            (string name, string number) = visibleChart.ParseDifficultyName(maxNumberLength: 3);
            difficulty.text = string.IsNullOrEmpty(number) ? name : InterfaceUtility.AlignedDiffNumber(number);

            foreach (var im in difficultyCells)
            {
                difficultyCellPool.Return(im);
            }

            foreach (var chart in level.Settings.Charts)
            {
                if (chart.ChartPath == visibleChart.ChartPath)
                {
                    continue;
                }

                DifficultyCell im = difficultyCellPool.Get(difficultyCellParent);
                ColorUtility.TryParseHtmlString(chart.DifficultyColor, out Color c);
                im.Color = c;
                difficultyCells.Add(im);
            }

            if (storage.TryAssignTextureFromCache(jacket, level, visibleChart.JacketPath))
            {
                MarkFullyLoaded();
            }
        }

        private void Awake()
        {
            storage.SelectedChart.OnValueChange += OnChartChange;
            defaultSizeDelta = rect.sizeDelta;
        }

        private void OnDestroy()
        {
            storage.SelectedChart.OnValueChange -= OnChartChange;
        }

        private void OnChartChange((LevelStorage level, ChartSettings chart) obj)
        {
            var (level, chart) = obj;
            UpdateSelectedState(InterfaceUtility.AreTheSame(level, this.level));
        }

        private void UpdateSelectedState(bool isSelected)
        {
            playIndicate.SetActive(isSelected);
            difficulty.gameObject.SetActive(!isSelected);

            rect.DOSizeDelta(isSelected ? selectedSizeDelta : defaultSizeDelta, animationDuration).SetEase(animationEase);
            this.isSelected = isSelected;
        }

        private void UpdateSelectedStateImmediate(bool isSelected)
        {
            playIndicate.SetActive(isSelected);
            difficulty.gameObject.SetActive(!isSelected);

            rect.DOKill();
            rect.localScale = Vector3.one;
            rect.sizeDelta = isSelected ? selectedSizeDelta : defaultSizeDelta;
            this.isSelected = isSelected;
        }
    }
}