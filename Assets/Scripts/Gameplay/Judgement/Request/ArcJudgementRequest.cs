using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Judgement
{
    public struct ArcJudgementRequest
    {
        public int ExpireAtTiming;
        public int AutoAtTiming;
        public Arc Arc;
        public IArcJudgementReceiver Receiver;
    }
}