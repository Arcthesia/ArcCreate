using ArcCreate.Utility;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class AutoInputHandler : IInputHandler
    {
        public void PollInput()
        {
        }

        public void HandleLaneTapRequests(int currentTiming, UnorderedList<LaneTapJudgementRequest> requests)
        {
            for (int i = requests.Count - 1; i >= 0; i--)
            {
                LaneTapJudgementRequest req = requests[i];
                if (currentTiming >= req.AutoAtTiming)
                {
                    req.Receiver.ProcessLaneTapJudgement(0);
                    requests.RemoveAt(i);
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
                    req.Receiver.ProcessLaneHoldJudgement(0);
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
                    req.Receiver.ProcessArcJudgement(0);
                    requests.RemoveAt(i);
                }
            }
        }
    }
}