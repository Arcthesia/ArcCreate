using ArcCreate.Utility;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class AutoInputHandler : IInputHandler
    {
        public void PollInput()
        {
        }

        public void HandleTapRequests(
            int currentTiming,
            UnorderedList<LaneTapJudgementRequest> laneTapRequests,
            UnorderedList<ArcTapJudgementRequest> arcTapRequests)
        {
            for (int i = laneTapRequests.Count - 1; i >= 0; i--)
            {
                LaneTapJudgementRequest req = laneTapRequests[i];
                if (currentTiming >= req.AutoAtTiming)
                {
                    req.Receiver.ProcessLaneTapJudgement(0, req.Properties);
                    laneTapRequests.RemoveAt(i);
                }
            }

            for (int i = arcTapRequests.Count - 1; i >= 0; i--)
            {
                ArcTapJudgementRequest req = arcTapRequests[i];
                if (currentTiming >= req.AutoAtTiming)
                {
                    req.Receiver.ProcessArcTapJudgement(0, req.Properties);
                    arcTapRequests.RemoveAt(i);
                }
            }
        }

        public void HandleLaneHoldRequests(int currentTiming, UnorderedList<LaneHoldJudgementRequest> requests)
        {
            for (int i = requests.Count - 1; i >= 0; i--)
            {
                LaneHoldJudgementRequest req = requests[i];
                if (currentTiming >= req.AutoAtTiming)
                {
                    req.Receiver.ProcessLaneHoldJudgement(false, req.IsJudgement, req.Properties);
                    requests.RemoveAt(i);
                }
            }
        }

        public void HandleArcRequests(int currentTiming, UnorderedList<ArcJudgementRequest> requests)
        {
            for (int i = requests.Count - 1; i >= 0; i--)
            {
                ArcJudgementRequest req = requests[i];
                if (currentTiming >= req.AutoAtTiming)
                {
                    req.Receiver.ProcessArcJudgement(false, req.IsJudgement, req.Properties);
                    requests.RemoveAt(i);
                }
            }
        }

        public void ResetJudge()
        {
        }
    }
}