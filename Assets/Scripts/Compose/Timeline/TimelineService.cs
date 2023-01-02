using UnityEngine;

namespace ArcCreate.Compose.Timeline
{
    public class TimelineService : MonoBehaviour, ITimelineService
    {
        [SerializeField] private WaveformDisplay waveformDisplay;
    }
}