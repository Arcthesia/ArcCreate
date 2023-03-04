using System.Collections.Generic;
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
        private readonly char[] scoreCharArray = new char[64];
        private readonly char[] comboCharArray = new char[64];

        private int currentCombo = 0;
        private int totalCombo = 1;
        private double currentScoreFull = 0;
        private double currentScorePartial = 0;
        private float comboRedmix = 0;
        private readonly UnorderedList<ScoreEvent> pendingScoreEvents = new UnorderedList<ScoreEvent>(20);
        private readonly List<JudgementResult> resultReceivedThisFrame = new List<JudgementResult>(20);

        public int CurrentScore => CurrentScoreTotal;

        public int CurrentCombo => currentCombo;

        private int CurrentScoreTotal => (int)System.Math.Round(currentScoreFull + currentScorePartial);

        public void ProcessJudgement(JudgementResult result)
        {
            resultReceivedThisFrame.Add(result);

            if (result.IsLost())
            {
                currentCombo = 0;
                if (Mathf.Approximately(comboRedmix, 0))
                {
                    comboRedmix = 1;
                }

                return;
            }

            comboRedmix = 0;
            currentCombo++;

            double scorePerNote =
                totalCombo != 0 ? (double)Values.MaxScore / totalCombo : 0;

            double scoreToAdd = 0;
            if (result.IsFar())
            {
                scoreToAdd = scorePerNote * Values.FarPenaltyMultipler;
            }
            else if (result.IsMax())
            {
                scoreToAdd = scorePerNote + 1;
            }
            else
            {
                scoreToAdd = scorePerNote;
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

        public void ResetScoreTo(int currentCombo, int totalCombo)
        {
            this.currentCombo = currentCombo;
            this.totalCombo = totalCombo;
            SetCombo(currentCombo);

            pendingScoreEvents.Clear();
            currentScorePartial = 0;

            if (totalCombo == 0)
            {
                currentScoreFull = 0;
                SetScore(0);
            }
            else
            {
                double scorePerNote = (double)Values.MaxScore / totalCombo;
                currentScoreFull = (scorePerNote + 1) * currentCombo;
            }

            SetScore(CurrentScoreTotal);
        }

        public List<JudgementResult> GetJudgementsThisFrame() => resultReceivedThisFrame;

        public void ClearJudgementsThisFrame() => resultReceivedThisFrame.Clear();

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
    }
}