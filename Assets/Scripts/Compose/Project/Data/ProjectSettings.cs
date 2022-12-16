using System.Collections.Generic;

namespace ArcCreate.Compose.Project
{
    public class ProjectSettings
    {
        public string Path { get; set; }

        public string LastWorkingDifficulty { get; set; }

        public List<ChartSettings> Charts { get; set; }
    }
}