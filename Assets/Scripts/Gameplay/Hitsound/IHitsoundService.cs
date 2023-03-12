using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
        /// Gets the hitsound audio clip for tap notes.
        /// </summary>
        AudioClip TapHitsoundClip { get; }

        /// <summary>
        /// Gets the hitsound audio clip for arc notes.
        /// </summary>
        AudioClip ArcHitsoundClip { get; }

        /// <summary>
        /// Gets a map of sfx name to audioclip loaded for the current chart.
        /// </summary>
        Dictionary<string, AudioClip> SfxAudioClips { get; }

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