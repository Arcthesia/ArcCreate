using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;

namespace ArcCreate.Gameplay.Score
{
    /// <summary>
    /// Interface for providing score services to internal (Gameplay) classes.
    /// </summary>
    public interface IScoreService
    {
        /// <summary>
        /// Gets a list of judgement results that was sent to this service in a frame.
        /// </summary>
        /// <returns>List of judgement results.</returns>
        List<JudgementResult> GetJudgementsThisFrame();

        /// <summary>
        /// Process one or multiple judgement events.
        /// </summary>
        /// <param name="result">The result of the judgement.</param>
        void ProcessJudgement(JudgementResult result);

        /// <summary>
        /// Set the score and combo counter to the state with the specified combo counts. Used for auto mode.
        /// </summary>
        /// <param name="hitCount">The number of hit combo. Assumed to be all <see cref="JudgementResult.Max"/>.</param>
        /// <param name="totalCombo">The total combo of the chart.</param>
        void ResetScoreTo(int hitCount, int totalCombo);

        /// <summary>
        /// Update the score display.
        /// </summary>
        /// <param name="currentTiming">The current chart timing.</param>
        void UpdateDisplay(int currentTiming);
    }
}