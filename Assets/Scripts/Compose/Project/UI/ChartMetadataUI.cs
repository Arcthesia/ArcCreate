using ArcCreate.Data;
using UnityEngine;

namespace ArcCreate.Compose.Project
{
    public abstract class ChartMetadataUI : MonoBehaviour
    {
        protected ChartSettings Target { get; private set; }

        protected void Start()
        {
            Services.Project.OnChartLoad += OnChartLoad;
        }

        protected abstract void ApplyChartSettings(ChartSettings chart);

        private void OnChartLoad(ChartSettings chart)
        {
            Target = chart;
            ApplyChartSettings(chart);
        }
    }
}
