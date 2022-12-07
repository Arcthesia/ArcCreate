namespace ArcCreate.Gameplay.Judgement
{
    /// <summary>
    /// Interface for notes requesting a lane hold judgement.
    /// </summary>
    public interface ILaneHoldJudgementReceiver
    {
        /// <summary>
        /// Called when a judgement was processed by input handler.
        /// </summary>
        /// <param name="offset">Offset of judgement from auto timing.
        /// Positive is late, negative is early.</param>
        void ProcessLaneHoldJudgement(int offset);
    }
}