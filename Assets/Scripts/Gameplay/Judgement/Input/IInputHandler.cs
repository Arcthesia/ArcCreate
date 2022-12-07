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
        /// Handle lane tap requests. Make sure request lists are free from expired requests.
        /// </summary>
        /// <param name="currentTiming">The current audio timing.</param>
        /// <param name="laneTapRequests">List of lane tap requests.</param>
        void HandleTapRequests(int currentTiming, UnorderedList<LaneTapJudgementRequest> laneTapRequests);

        /// <summary>
        /// Handle lane hold requests. Make sure request lists are free from expired requests.
        /// </summary>
        /// <param name="currentTiming">The current audio timing.</param>
        /// <param name="requests">List of requests.</param>
        void HandleLaneHoldRequests(int currentTiming, UnorderedList<LaneHoldJudgementRequest> requests);
    }
}