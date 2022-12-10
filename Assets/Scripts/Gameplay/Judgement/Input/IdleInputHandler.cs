using ArcCreate.Utility;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class IdleInputHandler : IInputHandler
    {
        public void PollInput()
        {
        }

        public void HandleTapRequests(
            int currentTiming,
            UnorderedList<LaneTapJudgementRequest> laneTapRequests,
            UnorderedList<ArcTapJudgementRequest> arcTapRequests)
        {
        }

        public void HandleArcRequests(int currentTiming, UnorderedList<ArcJudgementRequest> requests)
        {
        }

        public void HandleLaneHoldRequests(int currentTiming, UnorderedList<LaneHoldJudgementRequest> requests)
        {
        }
    }
}