using ArcCreate.Gameplay.Data;

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
        /// <param name="isExpired">Whether the judgement request was expired.</param>
        /// <param name="isJudgement">Relayed from <see cref="ArcJudgementRequest.IsJudgement"/>.</param>
        /// <param name="properties">Relayed from <see cref="ArcJudgementRequest.Properties"/>.</param>
        void ProcessArcJudgement(bool isExpired, bool isJudgement, GroupProperties properties);
    }
}