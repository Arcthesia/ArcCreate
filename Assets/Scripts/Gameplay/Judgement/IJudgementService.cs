using UnityEngine;

namespace ArcCreate.Gameplay.Judgement
{
    /// <summary>
    /// Interface for providing input judgement services to internal (Gameplay) classes.
    /// </summary>
    public interface IJudgementService
    {
        /// <summary>
        /// Add a judgement request to be processed later.
        /// </summary>
        /// <param name="request">The request details.</param>
        void Request(TapJudgementRequest request);

        /// <summary>
        /// Start processing user input. Should be done after all notes requested for judgements as necessary.
        /// </summary>
        /// <param name="currentTiming">The current timing.</param>
        void ProcessInput(int currentTiming);
    }
}