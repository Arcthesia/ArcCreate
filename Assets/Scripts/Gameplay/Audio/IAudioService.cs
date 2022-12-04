namespace ArcCreate.Gameplay.Audio
{
    public interface IAudioService
    {
        int Timing { get; set; }

        int AudioLength { get; }

        bool IsPlaying { get; }

        /// <summary>
        /// Update the timing value.
        /// </summary>
        void UpdateTime();
    }
}