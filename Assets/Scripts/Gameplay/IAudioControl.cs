using System;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    public interface IAudioControl
    {
        /// <summary>
        /// Gets or sets the audio timing.
        /// Setting the timing will cause score to reset to 0.
        /// Additionally jumping to the new timing will cause the audio to be played immediately without delay
        /// which might cause audio lag. Consider <see cref="Pause"/> and <see cref="PlayWithDelay"/> if
        /// this becomes an issue.
        /// </summary>
        /// <value>The audio timing.</value>
        int Timing { get; set; }

        /// <summary>
        /// Gets the length in ms of the currently playing audio clip.
        /// </summary>
        /// <value>The length of the currently playing audio.</value>
        int AudioLength { get; }

        /// <summary>
        /// Gets a value indicating whether or not the audio is playing.
        /// </summary>
        /// <value>Whether or not the audio is playing.</value>
        bool IsPlaying { get; }

        /// <summary>
        /// Gets or sets the audio clip to play.
        /// </summary>
        /// <value>The audio clip.</value>
        AudioClip AudioClip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to stop the audio and return to 0ms after the audio clip ends.
        /// </summary>
        /// <value>The bool setting. True by default.</value>
        bool AutomaticallyReturnOnAudioEnd { get; set; }

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
        /// Load the audio clip from the specified path.
        /// </summary>
        /// <param name="path">The path to load.</param>
        void LoadAudio(string path);
    }
}