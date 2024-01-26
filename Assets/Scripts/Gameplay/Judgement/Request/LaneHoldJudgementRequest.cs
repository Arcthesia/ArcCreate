using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Judgement
{
    public struct LaneHoldJudgementRequest
    {
        public int StartAtTiming;
        public int ExpireAtTiming;
        public int AutoAtTiming;
        public int Lane;
        public bool IsJudgement;
        public ILaneHoldJudgementReceiver Receiver;
        public GroupProperties Properties;
    }
}