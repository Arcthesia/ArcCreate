using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace ArcCreate.Data
{
    public class ProjectSettings
    {
        [YamlIgnore]
        public string Path { get; set; }

        public string LastOpenedChartPath { get; set; }

        public List<ChartSettings> Charts { get; set; }
    }
}