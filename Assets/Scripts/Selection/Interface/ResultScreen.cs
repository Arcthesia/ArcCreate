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
        [SerializeField] private Camera resultCamera;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text composer;
        [SerializeField] private Image difficulty;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private GameObject charterFrame;
        [SerializeField] private RectTransform charterRect;
        [SerializeField] private TMP_Text charterName;
        [SerializeField] private GameObject aliasFrame;
        [SerializeField] private RectTransform aliasRect;
        [SerializeField] private TMP_Text aliasName;
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
        [SerializeField] private GradeDisplay gradeDisplay;
        [SerializeField] private ClearResultDisplay clearResultDisplay;
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
            difficulty.color = c;
            difficultyText.text = chart.Difficulty;
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
            gradeDisplay.Display(play.Grade);
            clearResultDisplay.Display(play.ClearResult);
            playCount.text = play.PlayCount.ToString();
            retryCount.text = play.RetryCount.ToString();

            double best = play.BestScore;
            double current = play.Score;
            bestScore.text = PlayResult.FormatScore(best);
            scoreIncrease.text = (current >= best ? "+" : "") + PlayResult.FormatScore(current - best);
            score.text = PlayResult.FormatScore(current);

            charterFrame.SetActive(!string.IsNullOrEmpty(chart.Charter));
            aliasFrame.SetActive(!string.IsNullOrEmpty(chart.Alias));
            charterName.text = chart.Charter ?? string.Empty;
            aliasName.text = chart.Alias ?? string.Empty;
            aliasRect.offsetMax = new Vector2(aliasRect.offsetMax.x, -charterName.preferredHeight);

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
            TransitionScene.Instance.SetTargetCamera(resultCamera, "Default");
        }

        private void ReturnToPreviousScene()
        {
            // lol
            TransitionSequence transition = new TransitionSequence();
            SceneTransitionManager.Instance.SetTransition(transition);
            SceneTransitionManager.Instance.SwitchScene(SceneNames.SelectScene).Forget();
        }

        private void RetryChart()
        {
            if (currentLevel != null && currentChart != null)
            {
                storage.SwitchToPlayScene((currentLevel, currentChart));
            }
        }
    }
}