using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Judgement
{
    public struct TapJudgementRequest
    {
        public int ExpireAtTiming;
        public int AutoAt;
        public int Lane;
        public Tap Receiver;
    }
}