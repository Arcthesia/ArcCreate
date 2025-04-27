using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Judgement
{
    public struct LaneHoldJudgementRequest
    {
        public int StartAtTiming;
        public int ExpireAtTiming;
        public int AutoAtTiming;
        public float Lane;
        public bool IsJudgement;
        public Hold Receiver;
        public GroupProperties Properties;
    }
}