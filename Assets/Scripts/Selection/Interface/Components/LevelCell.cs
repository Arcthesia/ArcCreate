using System.Collections.Generic;
using System.Threading;
using ArcCreate.Data;
using ArcCreate.Selection.Select;
using ArcCreate.Selection.SoundEffect;
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
        [SerializeField] private Image border;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text composer;
        [SerializeField] private TMP_Text difficulty;
        [SerializeField] private RawImage jacket;
        [SerializeField] private RawImage jacketFill;
        [SerializeField] private DifficultyCell difficultyBackground;
        [SerializeField] private Image difficultyImageNormal;
        [SerializeField] private Image difficultyImageSelected;
        [SerializeField] private ClearResultDisplay clearResult;
        [SerializeField] private GradeDisplay grade;
        [SerializeField] private GameObject newIndicator;
        [SerializeField] private Transform difficultyCellParent;
        [SerializeField] private GameObject playIndicate;

        [Header("Animation")]
        [SerializeField] private RectTransform rect;
        [SerializeField] private Vector2 selectedSizeDelta;
        [SerializeField] private float animationDuration = 0.15f;
        [SerializeField] private Ease animationEase = Ease.OutExpo;

        private LevelStorage level;
        private PlayHistory playHistory;
        private readonly List<DifficultyCell> difficultyCells = new List<DifficultyCell>();
        private ChartSettings visibleChart;
        private Vector2 defaultSizeDelta;
        private bool isSelected;
        private Color diffColor;

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
                Services.SoundEffect.Play(Sound.CellSelect);
            }
        }

        public override void SetCellData(CellData cellData)
        {
            LevelCellData data = cellData as LevelCellData;
            level = data.LevelStorage;
            playHistory = data.PlayHistory;
            selectable.StorageUnit = level;
            visibleChart = data.ChartToDisplay;

            ColorUtility.TryParseHtmlString(visibleChart.DifficultyColor, out diffColor);
            difficultyBackground.Color = diffColor;
            UpdateSelectedStateImmediate(InterfaceUtility.AreTheSame(level, storage.SelectedChart.Value.level));
            if (isSelected)
            {
                visibleChart = storage.SelectedChart.Value.chart;
            }

            difficultyCellPool = difficultyCellPool == null || difficultyCellPool.IsDestroyed ? Pools.Get<DifficultyCell>("DifficultyCell") : difficultyCellPool;
            SetInfo();
        }

        public override async UniTask LoadCellFully(CellData cellData, CancellationToken cancellationToken)
        {
            await storage.AssignTexture(jacket, level, visibleChart.JacketPath);
            jacketFill.texture = jacket.texture;
        }

        private void SetInfo()
        {
            title.text = visibleChart.Title;
            composer.text = visibleChart.Composer;

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

            jacketFill.texture = jacket.texture;
            grade.Display(playHistory.BestScorePlayOrDefault.Grade);
            clearResult.Display(playHistory.BestResultPlayOrDefault.ClearResult);
            grade.gameObject.SetActive(playHistory.PlayCount > 0);
            clearResult.gameObject.SetActive(playHistory.PlayCount > 0);
            newIndicator.SetActive(playHistory.PlayCount <= 0);
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
            border.DOColor(isSelected ? Color.white : new Color(1, 1, 1, 0.5f), animationDuration).SetEase(animationEase);

            Color diffColorClear = diffColor;
            diffColorClear.a = 0;
            difficultyImageNormal.DOColor(isSelected ? diffColorClear : diffColor, animationDuration).SetEase(animationEase);
            difficultyImageSelected.DOColor(isSelected ? diffColor : diffColorClear, animationDuration).SetEase(animationEase);
            this.isSelected = isSelected;
        }

        private void UpdateSelectedStateImmediate(bool isSelected)
        {
            playIndicate.SetActive(isSelected);
            difficulty.gameObject.SetActive(!isSelected);

            rect.DOKill();
            rect.localScale = Vector3.one;
            rect.sizeDelta = isSelected ? selectedSizeDelta : defaultSizeDelta;

            border.DOKill();
            border.color = isSelected ? Color.white : new Color(1, 1, 1, 0.5f);

            Color diffColorClear = diffColor;
            diffColorClear.a = 0;

            difficultyImageNormal.DOKill();
            difficultyImageNormal.color = isSelected ? diffColorClear : diffColor;

            difficultyImageSelected.DOKill();
            difficultyImageSelected.color = isSelected ? diffColor : diffColorClear;
            this.isSelected = isSelected;
        }
    }
}