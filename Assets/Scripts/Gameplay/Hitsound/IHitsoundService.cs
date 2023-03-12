using Cysharp.Threading.Tasks;

namespace ArcCreate.Gameplay.Hitsound
{
    /// <summary>
    /// Interface for providing hitsound playing services to internal (Gameplay) classes.
    /// </summary>
    public interface IHitsoundService
    {
        /// <summary>
        /// Gets a value indicating whether or not all hitsounds have been loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Plays the tap hitsound.
        /// </summary>
        void PlayTapHitsound();

        /// <summary>
        /// Plays the arc hitsound.
        /// </summary>
        void PlayArcHitsound();

        /// <summary>
        /// Plays an arctap hitsound with the provided sfx, or plays arc hitsound if the sfx is not valid.
        /// </summary>
        /// <param name="sfx">The sfx to play.</param>
        /// <param name="isFromJudgement">Whether this was invoked as a feedback to a judgement, or was invoked on the note's timing.</param>
        void PlayArcTapHitsound(string sfx, bool isFromJudgement);

        UniTask LoadCustomSfxs(string sfxParentUri);
    }
}