using ArcCreate.Utility;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public interface IInputHandler
    {
        /// <summary>
        /// Start polling from input devices.
        /// </summary>
        void PollInput();

        /// <summary>
        /// Handle tap requests. Make sure request lists are free from expired requests.
        /// </summary>
        /// <param name="currentTiming">The current audio timing.</param>
        /// <param name="tapRequests">List of tap requests.</param>
        void HandleTaps(int currentTiming, UnorderedList<TapJudgementRequest> tapRequests);
    }
}