using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Judgement
{
    /// <summary>
    /// Interface for notes requesting an arctap judgement.
    /// </summary>
    public interface IArcTapJudgementReceiver
    {
        /// <summary>
        /// Called when a judgement was processed by input handler.
        /// </summary>
        /// <param name="offset">Offset of judgement from auto timing.
        /// Positive is late, negative is early.</param>
        /// <param name="properties">Relayed from <see cref="ArcTapJudgementRequest.Properties"/>.</param>
        void ProcessArcTapJudgement(int offset, GroupProperties properties);
    }
}
