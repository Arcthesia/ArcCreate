using System;
using System.Collections.Generic;
using System.Threading;
using ArcCreate.Data;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;
using ArcCreate.Utility.LRUCache;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class LevelCell : Cell
    {
        private static readonly LRUCache<string, Sprite> JacketCache = new LRUCache<string, Sprite>(50, DestroySprite);

        private static Pool<Image> difficultyCellPool;
        [SerializeField] private Components.Selectable selectable;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text composer;
        [SerializeField] private TMP_Text difficulty;
        [SerializeField] private GameObject plusIcon;
        [SerializeField] private Image jacket;
        [SerializeField] private Image difficultyBackground;
        [SerializeField] private Transform difficultyCellParent;
        private LevelList levelList;
        private LevelStorage level;
        private readonly List<Image> difficultyImages = new List<Image>();
        private ChartSettings visibleChart;

        public override void SetCellData(CellData cellData)
        {
            LevelCellData data = cellData as LevelCellData;
            levelList = data.LevelList;
            level = data.LevelStorage;
            selectable.StorageUnit = level;

            difficultyCellPool = difficultyCellPool ?? Pools.Get<Image>("DifficultyCell");
            visibleChart = GetChartToDisplay();
            SetInfo();
        }

        public override async UniTask LoadCellFully(CellData cellData, CancellationToken cancellationToken)
        {
            if (jacket.sprite == null)
            {
                string jacketPath = level.GetRealPath(visibleChart.JacketPath);
                using (UnityWebRequest req = UnityWebRequestTexture.GetTexture("file:///" + Uri.EscapeDataString(jacketPath.Replace("\\", "/"))))
                {
                    (bool isCancelleed, UnityWebRequest result) = await req
                        .SendWebRequest()
                        .ToUniTask()
                        .AttachExternalCancellation(cancellationToken)
                        .SuppressCancellationThrow();

                    if (!isCancelleed && !string.IsNullOrEmpty(req.error))
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(req);
                        Sprite sprite = Sprite.Create(
                            texture: texture,
                            rect: new Rect(0, 0, texture.width, texture.height),
                            pivot: new Vector2(0.5f, 0.5f));

                        jacket.sprite = sprite;
                        JacketCache.Add(jacketPath, sprite);
                    }
                }
            }
        }

        private static void DestroySprite(Sprite obj)
        {
            Destroy(obj.texture);
            Destroy(obj);
        }

        private ChartSettings GetChartToDisplay()
        {
            ChartSettings selectedChart = levelList.SelectedChart;
            ChartSettings currentChart = null;

            if (selectedChart != null)
            {
                foreach (var chart in level.Settings.Charts)
                {
                    if (chart.IsSameDifficulty(selectedChart))
                    {
                        currentChart = chart;
                        break;
                    }
                }
            }

            if (currentChart == null && selectedChart != null)
            {
                float minCcDiff = float.MaxValue;
                foreach (var chart in level.Settings.Charts)
                {
                    float ccDiff = chart.ChartConstant - selectedChart.ChartConstant;
                    if (ccDiff < minCcDiff)
                    {
                        currentChart = chart;
                        minCcDiff = ccDiff;
                    }
                }
            }

            return currentChart ?? level.Settings.Charts[0];
        }

        private void SetInfo()
        {
            title.text = visibleChart.Title;
            composer.text = visibleChart.Composer;
            ColorUtility.TryParseHtmlString(visibleChart.DifficultyColor, out Color color);
            difficultyBackground.color = color;

            (int diff, bool isPlus) = visibleChart.ParseChartConstant();
            difficulty.text = diff.ToString();
            plusIcon.SetActive(isPlus);

            foreach (var im in difficultyImages)
            {
                difficultyCellPool.Return(im);
            }

            foreach (var chart in level.Settings.Charts)
            {
                if (chart == visibleChart)
                {
                    continue;
                }

                Image im = difficultyCellPool.Get(difficultyCellParent);
                ColorUtility.TryParseHtmlString(chart.DifficultyColor, out Color c);
                im.color = c;
                difficultyImages.Add(im);
            }

            string jacketPath = level.GetRealPath(visibleChart.JacketPath);
            Sprite cachedJacket = JacketCache.Get(jacketPath);
            if (cachedJacket != null)
            {
                jacket.sprite = cachedJacket;
                MarkFullyLoaded();
            }
            else
            {
                jacket.sprite = null;
            }
        }
    }
}