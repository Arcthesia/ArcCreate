using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Judgement
{
    public struct ArcTapJudgementRequest
    {
        public int ExpireAtTiming;
        public int AutoAtTiming;
        public float X;
        public float Y;
        public float Width;
        public bool IsBlocker;
        public IArcTapJudgementReceiver Receiver;
        public GroupProperties Properties;
    }
}