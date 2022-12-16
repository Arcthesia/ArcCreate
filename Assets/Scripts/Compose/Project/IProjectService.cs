using UnityEngine.Events;

namespace ArcCreate.Compose.Project
{
    public interface IProjectService
    {
        UnityEvent<ProjectSettings> OnProjectLoad { get; }

        UnityEvent<ChartSettings> OnChartLoad { get; }

        ProjectSettings CurrentProject { get; }

        ChartSettings CurrentChart { get; }
    }
}