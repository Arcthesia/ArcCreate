namespace ArcCreate.Compose.Timeline
{
    /// <summary>
    /// Services for handling timeline display and audio playback.
    /// </summary>
    public interface ITimelineService
    {
        int ViewFromTiming { get; }

        int ViewToTiming { get; }
    }
}