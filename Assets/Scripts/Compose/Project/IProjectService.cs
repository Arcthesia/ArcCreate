using System;

namespace ArcCreate.Compose.Project
{
    public interface IProjectService
    {
        event Action<ChartSettings> OnChartLoad;

        ProjectSettings CurrentProject { get; }

        ChartSettings CurrentChart { get; }

        void CreateNewProject(ProjectSettings project);

        void CreateNewChart(string name);
    }
}