using System;
using System.Text;
using System.Threading;
using ArcCreate.Data;
using ArcCreate.SceneTransition;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    public class ResultScreen : SceneRepresentative
    {
        [SerializeField] private StorageData storage;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text composer;
        [SerializeField] private DifficultyCell difficulty;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private TMP_Text charterName;
        [SerializeField] private TMP_Text charterAlias;
        [SerializeField] private GameObject charterAliasLabel;
        [SerializeField] private RawImage jacket;
        [SerializeField] private TMP_Text perfectEarly;
        [SerializeField] private TMP_Text perfectTotal;
        [SerializeField] private TMP_Text perfectLate;
        [SerializeField] private TMP_Text goodEarly;
        [SerializeField] private TMP_Text goodTotal;
        [SerializeField] private TMP_Text goodLate;
        [SerializeField] private TMP_Text missEarly;
        [SerializeField] private TMP_Text missTotal;
        [SerializeField] private TMP_Text missLate;
        [SerializeField] private TMP_Text maxCombo;
        [SerializeField] private TMP_Text playCount;
        [SerializeField] private TMP_Text retryCount;
        [SerializeField] private TMP_Text bestScore;
        [SerializeField] private TMP_Text scoreIncrease;
        [SerializeField] private TMP_Text score;
        [SerializeField] private Image background;
        [SerializeField] private Sprite[] backgroundSprites;
        [SerializeField] private Image side;
        [SerializeField] private Sprite[] sideSprites;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private AudioPreview audioPreview;
        private LevelStorage currentLevel;
        private ChartSettings currentChart;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public void Display(LevelStorage level, ChartSettings chart, PlayResult play)
        {
            currentLevel = level;
            currentChart = chart;
            title.text = chart.Title;
            composer.text = chart.Composer;
            ColorUtility.TryParseHtmlString(chart.DifficultyColor, out Color c);
            difficulty.Color = c;
            difficultyText.text = chart.Difficulty;
            charterName.text = chart.Charter;
            charterName.gameObject.SetActive(!string.IsNullOrEmpty(chart.Charter));
            charterAlias.text = chart.Alias;
            charterAlias.gameObject.SetActive(!string.IsNullOrEmpty(chart.Alias));
            charterAliasLabel.SetActive(!string.IsNullOrEmpty(chart.Alias));
            storage.ReleasePersistent(jacket.texture);
            storage.AssignTexture(jacket, level, chart.JacketPath).Forget();
            storage.EnsurePersistent(jacket.texture);
            perfectEarly.text = play.EarlyPerfectCount.ToString();
            perfectTotal.text = play.PerfectCount.ToString();
            perfectLate.text = play.LatePerfectCount.ToString();
            goodEarly.text = play.EarlyGoodCount.ToString();
            goodTotal.text = play.GoodCount.ToString();
            goodLate.text = play.LateGoodCount.ToString();
            missEarly.text = play.EarlyMissCount.ToString();
            missTotal.text = play.MissCount.ToString();
            missLate.text = play.LateMissCount.ToString();
            maxCombo.text = play.MaxCombo.ToString();

            string sideString = (chart.Skin?.Side ?? "").ToLower();
            switch (sideString)
            {
                case "light":
                    side.sprite = sideSprites[0];
                    background.sprite = backgroundSprites[0];
                    break;
                case "conflict":
                    side.sprite = sideSprites[1];
                    background.sprite = backgroundSprites[1];
                    break;
                case "colorless":
                    side.sprite = sideSprites[2];
                    background.sprite = backgroundSprites[2];
                    break;
                default:
                    side.sprite = sideSprites[0];
                    background.sprite = backgroundSprites[3];
                    break;
            }

            playCount.text = $"PLAY: {play.PlayCount}";
            retryCount.text = $"RETRY: {play.RetryCount}";

            double best = play.BestScore;
            bestScore.text = FormatScore(best);

            double current = play.Score;
            scoreIncrease.text = (current >= best ? "+" : "") + FormatScore(current - best);
            score.text = FormatScore(current);

            audioPreview.PlayPreviewAudio(level, chart, cts.Token).Forget();
        }

        public override void PassData(params object[] args)
        {
            LevelStorage level = args[0] as LevelStorage;
            ChartSettings chart = args[1] as ChartSettings;
            PlayResult result = (PlayResult)args[2];
            Display(level, chart, result);
        }

        public override void OnUnloadScene()
        {
            returnButton.onClick.RemoveListener(ReturnToPreviousScene);
            retryButton.onClick.RemoveListener(RetryChart);
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        protected override void OnSceneLoad()
        {
            returnButton.onClick.AddListener(ReturnToPreviousScene);
            retryButton.onClick.AddListener(RetryChart);
        }

        private void ReturnToPreviousScene()
        {
            SceneTransitionManager.Instance.SwitchScene(SceneNames.SelectScene).Forget();
        }

        private void RetryChart()
        {
            if (currentLevel != null && currentChart != null)
            {
                storage.SwitchToPlayScene((currentLevel, currentChart));
            }
        }

        private string FormatScore(double score)
        {
            string s = ((int)Math.Round(score)).ToString("D8");
            StringBuilder sb = new StringBuilder();
            for (int i = s.Length - 1; i >= 0; i--)
            {
                sb.Insert(0, s[i]);
                if ((i + 1) % 3 == 0 && i != 0)
                {
                    sb.Insert(0, '\'');
                }
            }

            return sb.ToString();
        }
    }
}