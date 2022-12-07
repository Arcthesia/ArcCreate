namespace ArcCreate.Gameplay.Judgement
{
    public struct LaneTapJudgementRequest
    {
        public int ExpireAtTiming;
        public int AutoAt;
        public int Lane;
        public ILaneTapJudgementReceiver Receiver;
    }
}