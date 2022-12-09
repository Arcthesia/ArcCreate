namespace ArcCreate.Gameplay.Judgement
{
    /// <summary>
    /// Interface for notes requesting an arc judgement.
    /// </summary>
    public interface IArcJudgementReceiver
    {
        /// <summary>
        /// Called when a judgement was processed by input handler.
        /// </summary>
        /// <param name="offset">Offset of judgement from auto timing.
        /// Positive is late, negative is early.</param>
        void ProcessArcJudgement(int offset);
    }
}