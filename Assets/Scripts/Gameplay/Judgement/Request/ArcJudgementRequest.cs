using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Judgement
{
    public struct ArcJudgementRequest
    {
        public int StartAtTiming;
        public int ExpireAtTiming;
        public int AutoAtTiming;
        public Arc Arc;
        public bool IsJudgement;
        public IArcJudgementReceiver Receiver;
        public GroupProperties Properties;
    }
}