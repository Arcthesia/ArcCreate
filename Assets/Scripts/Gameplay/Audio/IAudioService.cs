using UnityEngine;

namespace ArcCreate.Gameplay.Audio
{
    public interface IAudioService : IAudioControl
    {
        AudioSource AudioSource { get; }

        /// <summary>
        /// Gets a value indicating whether or not an audio clip has been loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Update the timing value.
        /// </summary>
        void UpdateTime();
    }
}