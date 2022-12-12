using ArcCreate.Utility;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class MouseInputHandler : IInputHandler
    {
        public void PollInput()
        {
            throw new System.NotImplementedException();
        }

        public void HandleTapRequests(
            int currentTiming,
            UnorderedList<LaneTapJudgementRequest> laneTapRequests,
            UnorderedList<ArcTapJudgementRequest> arcTapRequests)
        {
            throw new System.NotImplementedException();
        }

        public void HandleArcRequests(int currentTiming, UnorderedList<ArcJudgementRequest> requests)
        {
            throw new System.NotImplementedException();
        }

        public void HandleLaneHoldRequests(int currentTiming, UnorderedList<LaneHoldJudgementRequest> requests)
        {
            throw new System.NotImplementedException();
        }

        public void ResetJudge()
        {
            throw new System.NotImplementedException();
        }
    }
}