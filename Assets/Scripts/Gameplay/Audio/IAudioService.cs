namespace ArcCreate.Gameplay.Audio
{
    public interface IAudioService : IAudioControl
    {
        /// <summary>
        /// Update the timing value.
        /// </summary>
        void UpdateTime();
    }
}