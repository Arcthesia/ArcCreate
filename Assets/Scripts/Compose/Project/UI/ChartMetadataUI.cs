using ArcCreate.Data;
using ArcCreate.Gameplay;
using UnityEngine;

namespace ArcCreate.Compose.Project
{
    public abstract class ChartMetadataUI : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;

        protected ChartSettings Target { get; private set; }

        protected GameplayData GameplayData => gameplayData;

        protected abstract void ApplyChartSettings(ChartSettings chart);

        protected void Start()
        {
            gameplayData.OnChartFileLoad += OnChartLoad;
        }

        private void OnChartLoad()
        {
            Target = Services.Project.CurrentChart;
            ApplyChartSettings(Target);
        }
    }
}
