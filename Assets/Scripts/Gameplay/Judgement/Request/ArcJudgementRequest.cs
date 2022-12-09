namespace ArcCreate.Gameplay.Judgement
{
    public struct ArcJudgementRequest
    {
        public int ExpireAtTiming;
        public int AutoAtTiming;
        public float X;
        public float Y;
        public IArcJudgementReceiver Receiver;
    }
}