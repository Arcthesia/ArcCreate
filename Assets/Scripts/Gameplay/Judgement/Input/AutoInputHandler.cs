using ArcCreate.Utility;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class AutoInputHandler : IInputHandler
    {
        public void PollInput()
        {
        }

        public void HandleTaps(int currentTiming, UnorderedList<TapJudgementRequest> tapRequests)
        {
            for (int i = tapRequests.Count - 1; i >= 0; i--)
            {
                TapJudgementRequest req = tapRequests[i];
                if (currentTiming >= req.AutoAt)
                {
                    req.Receiver.ProcessJudgement(JudgementResult.Max);
                    tapRequests.RemoveAt(i);
                }
            }
        }
    }
}