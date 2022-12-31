using ArcCreate.Compose.Components;

namespace ArcCreate.Compose.Project
{
    public struct NewProjectInfo
    {
        public FilePath ProjectFile;
        public string StartingChartPath;
        public float BaseBPM;
        public FilePath AudioPath;
        public FilePath JacketPath;
        public FilePath BackgroundPath;
    }
}