using System.Collections.Generic;
using ArcCreate.ChartFormat;
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
        /// <param name="noteTiming">The timing of the note playing this hitsound.</param>
        void PlayTapHitsound(int noteTiming);

        /// <summary>
        /// Plays the arc hitsound.
        /// </summary>
        /// <param name="noteTiming">The timing of the note playing this hitsound.</param>
        void PlayArcHitsound(int noteTiming);

        /// <summary>
        /// Plays an arctap hitsound with the provided sfx, or plays arc hitsound if the sfx is not valid.
        /// </summary>
        /// <param name="noteTiming">The timing of the note playing this hitsound.</param>
        /// <param name="sfx">The sfx to play.</param>
        /// <param name="isFromJudgement">Whether this was invoked as a feedback to a judgement, or was invoked on the note's timing.</param>
        void PlayArcTapHitsound(int noteTiming, string sfx, bool isFromJudgement);

        /// <summary>
        /// Loads all sfx file referenced by the currently loaded chart from sfxParentUri.
        /// </summary>
        /// <param name="sfxParentUri">Where to load sfx files from.</param>
        /// <param name="fileAccess">Custom file accessor.</param>
        /// <returns>Unitask instance.</returns>
        UniTask LoadCustomSfxs(string sfxParentUri, IFileAccessWrapper fileAccess);

        void UpdateHitsoundHistory(int currentTiming);

        void ResetHitsoundHistory();
    }
}