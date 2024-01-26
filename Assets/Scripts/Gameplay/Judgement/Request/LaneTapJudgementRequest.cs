using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Judgement
{
    public struct LaneTapJudgementRequest
    {
        public int ExpireAtTiming;
        public int AutoAtTiming;
        public int Lane;
        public ILaneTapJudgementReceiver Receiver;
        public GroupProperties Properties;
    }
}