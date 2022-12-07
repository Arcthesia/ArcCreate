using ArcCreate.Utility;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class AutoInputHandler : IInputHandler
    {
        public void PollInput()
        {
        }

        public void HandleTapRequests(int currentTiming, UnorderedList<LaneTapJudgementRequest> tapRequests)
        {
            for (int i = tapRequests.Count - 1; i >= 0; i--)
            {
                LaneTapJudgementRequest req = tapRequests[i];
                if (currentTiming >= req.AutoAt)
                {
                    req.Receiver.ProcessLaneTapJudgement(0);
                    tapRequests.RemoveAt(i);
                }
            }
        }

        public void HandleLaneHoldRequests(int currentTiming, UnorderedList<LaneHoldJudgementRequest> holdRequests)
        {
            for (int i = holdRequests.Count - 1; i >= 0; i--)
            {
                LaneHoldJudgementRequest req = holdRequests[i];
                if (currentTiming >= req.AutoAt)
                {
                    req.Receiver.ProcessLaneHoldJudgement(0);
                    holdRequests.RemoveAt(i);
                }
            }
        }
    }
}