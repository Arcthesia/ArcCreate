namespace ArcCreate.Gameplay.Judgement
{
    public struct LaneHoldJudgementRequest
    {
        public int ExpireAtTiming;
        public int AutoAt;
        public int Lane;
        public ILaneHoldJudgementReceiver Receiver;
    }
}