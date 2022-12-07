using ArcCreate.Gameplay.Judgement;
using UnityEngine;

namespace ArcCreate.Gameplay.Score
{
    /// <summary>
    /// Interface for providing score services to internal (Gameplay) classes.
    /// </summary>
    public interface IScoreService
    {
        void ProcessJudgement(JudgementResult result, int count = 1);

        void ResetScoreTo(int currentCombo, int totalCombo);

        void UpdateDisplay(int currentTiming);
    }
}