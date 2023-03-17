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

        public ChartSettings GetClosestDifficultyToChart(ChartSettings selectedChart)
        {
            if (selectedChart == null)
            {
                return Charts[0];
            }

            return GetClosestDifficultyToConstant(selectedChart.ChartConstant);
        }

        public ChartSettings GetClosestDifficultyToConstant(double constant)
        {
            double minCcDiff = double.MaxValue;
            ChartSettings result = null;
            foreach (var chart in Charts)
            {
                double ccDiff = chart.ChartConstant - constant;
                if (ccDiff < minCcDiff)
                {
                    result = chart;
                    minCcDiff = ccDiff;
                }
            }

            return result;
        }
    }
}