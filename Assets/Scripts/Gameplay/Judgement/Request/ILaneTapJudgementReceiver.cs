using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Judgement
{
    /// <summary>
    /// Interface for notes requesting a lane tap judgement.
    /// </summary>
    public interface ILaneTapJudgementReceiver
    {
        /// <summary>
        /// Called when a judgement was processed by input handler.
        /// </summary>
        /// <param name="offset">Offset of judgement from auto timing.
        /// Positive is late, negative is early.</param>
        /// <param name="properties">Relayed from <see cref="LaneTapJudgementRequest.Properties"/>.</param>
        void ProcessLaneTapJudgement(int offset, GroupProperties properties);
    }
}