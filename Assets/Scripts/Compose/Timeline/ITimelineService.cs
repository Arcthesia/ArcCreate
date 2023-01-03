using UnityEngine;

namespace ArcCreate.Compose.Timeline
{
    public interface ITimelineService
    {
        int ViewFromTiming { get; }

        int ViewToTiming { get; }
    }
}