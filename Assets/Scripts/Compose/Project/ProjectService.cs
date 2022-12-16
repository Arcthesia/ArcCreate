using UnityEngine;
using UnityEngine.Events;

namespace ArcCreate.Compose.Project
{
    public class ProjectService : MonoBehaviour, IProjectService
    {
        public UnityEvent<ProjectSettings> OnProjectLoad { get; }
            = new OnProjectLoadEvent();

        public UnityEvent<ChartSettings> OnChartLoad { get; }
            = new OnChartLoadEvent();

        public ProjectSettings CurrentProject { get; }

        public ChartSettings CurrentChart { get; }

        public class OnProjectLoadEvent : UnityEvent<ProjectSettings>
        {
        }

        public class OnChartLoadEvent : UnityEvent<ChartSettings>
        {
        }
    }
}