using ArcCreate.Gameplay.Data;

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
        /// <param name="isExpired">Whether the judgement request was expired.</param>
        /// <param name="isJudgement">Relayed from <see cref="LaneHoldJudgementRequest.IsJudgement"/>.</param>
        /// <param name="properties">Relayed from <see cref="LaneHoldJudgementRequest.Properties"/>.</param>
        void ProcessLaneHoldJudgement(bool isExpired, bool isJudgement, GroupProperties properties);
    }
}