using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    public interface IAudioControl
    {
        /// <summary>
        /// Gets or sets the audio's actual timing.
        /// Setting this value will cause score to reset to 0.
        /// Additionally jumping to the new timing will cause the audio to be played immediately without delay
        /// which might cause audio lag. Consider <see cref="Pause"/> and <see cref="PlayWithDelay"/> if
        /// this becomes an issue.
        /// </summary>
        int AudioTiming { get; set; }

        /// <summary>
        /// Gets or sets the audio timing but offseted.
        /// Setting the timing will cause score to reset to 0.
        /// Additionally jumping to the new timing will cause the audio to be played immediately without delay
        /// which might cause audio lag. Consider <see cref="Pause"/> and <see cref="PlayWithDelay"/> if
        /// this becomes an issue.
        /// </summary>
        int ChartTiming { get; set; }

        /// <summary>
        /// Gets the length in ms of the currently playing audio clip.
        /// </summary>
        int AudioLength { get; }

        /// <summary>
        /// Gets a value indicating whether or not the audio is playing.
        /// </summary>
        bool IsPlaying { get; }

        AudioClip TapHitsoundClip { get; }

        AudioClip ArcHitsoundClip { get; }

        Dictionary<string, AudioClip> SfxAudioClips { get; }

        /// <summary>
        /// Play the audio immediately without delay. See <see cref="PlayWithDelay"/> for playing audio with minimal audio lag.
        /// </summary>
        /// <param name="timing">The timing from which to start playing.</param>
        void PlayImmediately(int timing);

        /// <summary>
        /// Play the audio after a set delay to minimize audio lag.
        /// </summary>
        /// <param name="timing">The timing from which to start playing.</param>
        /// <param name="delayMs">The delay after which audio starts playing.</param>
        void PlayWithDelay(int timing, int delayMs);

        /// <summary>
        /// Pause the audio.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stop the audio and return back to timing 0.
        /// </summary>
        void Stop();

        /// <summary>
        /// Resume the audio immediately without delay from the last paused timing.
        /// See <see cref="ResumeWithDelay"/> for playing audio with minimal audio lag.
        /// </summary>
        void ResumeImmediately();

        /// <summary>
        /// Resume the audio from the last paused timing, after a set delay to minimize audio lag.
        /// </summary>
        /// <param name="delayMs">The delay after which audio starts playing.</param>
        void ResumeWithDelay(int delayMs);

        /// <summary>
        /// Resume the audio immediately without delay from the last paused timing.
        /// Additionally return back to the timing at which this was called once <see cref="Pause"/> is called.
        /// See <see cref="ResumeReturnableWithDelay"/> for playing audio with minimal audio lag.
        /// </summary>
        void ResumeReturnableImmediately();

        /// <summary>
        /// Resume the audio from the last paused timing, after a set delay to minimize audio lag.
        /// Additionally return back to the timing at which this was called once <see cref="Pause"/> is called.
        /// </summary>
        /// <param name="delayMs">The delay after which audio starts playing.</param>
        void ResumeReturnableWithDelay(int delayMs);

        /// <summary>
        /// Set the timing to jump to next resuming.
        /// </summary>
        /// <param name="timing">The timing value.</param>
        void SetResumeAt(int timing);

        /// <summary>
        /// Set whether or not to return to a timing value after pausing.
        /// </summary>
        /// <param name="cond">Whether or not to return.</param>
        /// <param name="timing">The timing to return to.</param>
        void SetReturnOnPause(bool cond, int timing = 0);
    }
}