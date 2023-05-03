using System.Collections.Generic;
using ArcCreate.Data;
using ArcCreate.Gameplay.Judgement;
using ArcCreate.Utility;
using ArcCreate.Utility.Extension;
using TMPro;
using UnityEngine;

namespace ArcCreate.Gameplay.Score
{
    public class ScoreService : MonoBehaviour, IScoreService
    {
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Color comboLostColor;

        [Header("Indicator")]
        [SerializeField] private GameObject frIndicator;
        [SerializeField] private GameObject pmIndicator;
        [SerializeField] private GameObject maxIndicator;
        [SerializeField] private Transform indicatorsContainer;
        [SerializeField] private Transform[] indicatorParents;
        private readonly char[] scoreCharArray = new char[64];
        private readonly char[] comboCharArray = new char[64];

        private readonly int[] judgeCounts = new int[7];
        private int currentCombo = 0;
        private int maxCombo = 0;
        private int noteCount = 1;
        private double currentScoreFull = 0;
        private double currentScorePartial = 0;
        private float comboRedmix = 0;
        private readonly UnorderedList<ScoreEvent> pendingScoreEvents = new UnorderedList<ScoreEvent>(20);
        private readonly List<JudgementResult> resultReceivedThisFrame = new List<JudgementResult>(20);

        public int CurrentScore => CurrentScoreTotal;

        public int CurrentCombo => currentCombo;

        public int NoteCount => noteCount;

        private int CurrentScoreTotal => (int)System.Math.Round(currentScoreFull + currentScorePartial);

        public int GetJudgementCount(JudgementResult type)
        {
            return judgeCounts[(int)type];
        }

        public void SetJudgementCount(JudgementResult type, int count)
        {
            judgeCounts[(int)type] = count;
        }

        public void ProcessJudgement(JudgementResult result)
        {
            resultReceivedThisFrame.Add(result);
            SetJudgementCount(result, GetJudgementCount(result) + 1);

            if (result.IsMiss())
            {
                currentCombo = 0;
                if (Mathf.Approximately(comboRedmix, 0))
                {
                    comboRedmix = 1;
                }

                frIndicator.SetActive(false);
                pmIndicator.SetActive(false);
                maxIndicator.SetActive(false);
                return;
            }

            comboRedmix = 0;
            currentCombo++;
            maxCombo = Mathf.Max(currentCombo, maxCombo);

            double scorePerNote =
                noteCount != 0 ? (double)Constants.MaxScore / noteCount : 0;

            double scoreToAdd = 0;
            if (result.IsGood())
            {
                scoreToAdd = scorePerNote * Constants.GoodPenaltyMultipler;
                pmIndicator.SetActive(false);
                maxIndicator.SetActive(false);
            }
            else if (result.IsMax())
            {
                scoreToAdd = scorePerNote + 1;
            }
            else
            {
                scoreToAdd = scorePerNote;
                maxIndicator.SetActive(false);
            }

            pendingScoreEvents.Add(new ScoreEvent()
            {
                Timing = Services.Audio.ChartTiming,
                Score = scoreToAdd,
            });
        }

        public void UpdateDisplay(int currentTiming)
        {
            comboRedmix = comboRedmix - (Time.deltaTime / Values.ComboLostFlashDuration);
            comboRedmix = Mathf.Max(comboRedmix, 0);
            SetCombo(currentCombo);

            currentScorePartial = 0;
            for (int i = pendingScoreEvents.Count - 1; i >= 0; i--)
            {
                ScoreEvent scoreEvent = pendingScoreEvents[i];

                if (currentTiming > scoreEvent.Timing + Values.ScoreModifyDelay)
                {
                    pendingScoreEvents.RemoveAt(i);
                    currentScoreFull += scoreEvent.Score;
                }
                else
                {
                    double partial = scoreEvent.Score * (currentTiming - scoreEvent.Timing) / (double)Values.ScoreModifyDelay;
                    if (partial < 0)
                    {
                        partial = 0;
                    }

                    currentScorePartial += partial;
                }
            }

            SetScore(CurrentScoreTotal);
        }

        public void ResetScoreTo(int currentCombo, int noteCount)
        {
            this.currentCombo = currentCombo;
            this.maxCombo = currentCombo;
            this.noteCount = noteCount;
            SetCombo(currentCombo);

            pendingScoreEvents.Clear();
            currentScorePartial = 0;

            for (int i = 0; i < judgeCounts.Length; i++)
            {
                judgeCounts[i] = 0;
            }

            if (noteCount == 0)
            {
                currentScoreFull = 0;
                SetScore(0);
            }
            else
            {
                double scorePerNote = (double)Constants.MaxScore / noteCount;
                currentScoreFull = (scorePerNote + 1) * currentCombo;
                SetJudgementCount(JudgementResult.Max, currentCombo);
            }

            frIndicator.SetActive(true);
            pmIndicator.SetActive(true);
            maxIndicator.SetActive(Settings.EnableMaxIndicator.Value);
            SetScore(CurrentScoreTotal);
        }

        public List<JudgementResult> GetJudgementsThisFrame() => resultReceivedThisFrame;

        public void ClearJudgementsThisFrame() => resultReceivedThisFrame.Clear();

        public PlayResult GetPlayResult()
        {
            return new PlayResult
            {
                DateTime = System.DateTime.UtcNow,
                LateMissCount = judgeCounts[(int)JudgementResult.MissLate],
                LateGoodCount = judgeCounts[(int)JudgementResult.GoodLate],
                LatePerfectCount = judgeCounts[(int)JudgementResult.PerfectLate],
                MaxCount = judgeCounts[(int)JudgementResult.Max],
                EarlyPerfectCount = judgeCounts[(int)JudgementResult.PerfectEarly],
                EarlyGoodCount = judgeCounts[(int)JudgementResult.GoodEarly],
                EarlyMissCount = judgeCounts[(int)JudgementResult.MissEarly],
                MaxCombo = maxCombo,
                RetryCount = Values.RetryCount,
                GaugeValue = 100,
                GaugeClearRequirement = 70,
                GaugeMax = 100,
                NoteCount = noteCount,
            };
        }

        private void SetScore(int score)
        {
            scoreCharArray.SetNumberDigitsToArray(score, out int length);
            for (int i = scoreCharArray.Length - 8; i < scoreCharArray.Length - length; i++)
            {
                scoreCharArray[i] = '0';
            }

            length = Mathf.Max(length, 8);
            scoreText.SetCharArray(scoreCharArray, scoreCharArray.Length - length, length);
        }

        private void SetCombo(int combo)
        {
            if (Mathf.Approximately(comboRedmix, 0))
            {
                comboText.color = Services.Skin.ComboColor;
                comboText.outlineColor = Services.Skin.ComboColor;

                comboCharArray.SetNumberDigitsToArray(combo, out int length);
                comboText.SetCharArray(comboCharArray, comboCharArray.Length - length, length);
            }
            else
            {
                Color comboColor = comboLostColor;
                comboColor.a = comboRedmix;
                comboText.color = comboColor;
                comboText.outlineColor = comboColor;
            }
        }

        private void Awake()
        {
            Settings.EnableMaxIndicator.OnValueChanged.AddListener(OnEnableMaxIndicatorSettings);
            Settings.FrPmIndicatorPosition.OnValueChanged.AddListener(OnFrPmPositionSettings);

            OnEnableMaxIndicatorSettings(Settings.EnableMaxIndicator.Value);
            OnFrPmPositionSettings(Settings.FrPmIndicatorPosition.Value);
        }

        private void OnDestroy()
        {
            Settings.EnableMaxIndicator.OnValueChanged.RemoveListener(OnEnableMaxIndicatorSettings);
            Settings.FrPmIndicatorPosition.OnValueChanged.RemoveListener(OnFrPmPositionSettings);
        }

        private void OnEnableMaxIndicatorSettings(bool enable)
        {
            bool maxing = true;
            for (int i = 0; i < judgeCounts.Length; i++)
            {
                if ((JudgementResult)i != JudgementResult.Max && judgeCounts[i] > 0)
                {
                    maxing = false;
                }
            }

            maxIndicator.SetActive(enable && maxing);
        }

        private void OnFrPmPositionSettings(int pos)
        {
            var position = (FrPmPosition)pos;
            if (position == FrPmPosition.Off)
            {
                indicatorsContainer.gameObject.SetActive(false);
            }
            else
            {
                indicatorsContainer.gameObject.SetActive(true);
            }
        }
    }
}