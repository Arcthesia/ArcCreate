namespace ArcCreate.Gameplay.Judgement
{
    public struct LaneHoldJudgementRequest
    {
        public int ExpireAtTiming;
        public int AutoAtTiming;
        public int Lane;
        public ILaneHoldJudgementReceiver Receiver;
    }
}