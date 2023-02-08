using System.Collections.Generic;
using ArcCreate.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class ChartPicker : MonoBehaviour
    {
        [SerializeField] private GameObject chartRowPrefab;
        [SerializeField] private Transform rowsParent;
        [SerializeField] private GameObject chartPicker;
        [SerializeField] private Button togglePickerButton;
        [SerializeField] private Button newChartButton;
        [SerializeField] private NewChartDialog newChartDialog;
        private readonly List<ChartPickerRow> chartPickerRows = new List<ChartPickerRow>();

        public void SetOptions(List<ChartSettings> charts, ChartSettings currentChart)
        {
            foreach (ChartPickerRow row in chartPickerRows)
            {
                Destroy(row.gameObject);
            }

            chartPickerRows.Clear();

            foreach (ChartSettings chart in charts)
            {
                GameObject go = Instantiate(chartRowPrefab, rowsParent);
                ChartPickerRow row = go.GetComponent<ChartPickerRow>();
                row.SetChart(chart);
                row.SetCurrentlyActive(currentChart == chart);
                chartPickerRows.Add(row);
            }
        }

        private void Awake()
        {
            togglePickerButton.onClick.AddListener(TogglePicker);
            newChartButton.onClick.AddListener(OpenNewChartDialog);
        }

        private void OnDestroy()
        {
            togglePickerButton.onClick.RemoveListener(TogglePicker);
            newChartButton.onClick.RemoveListener(OpenNewChartDialog);
        }

        private void TogglePicker()
        {
            chartPicker.SetActive(!chartPicker.activeSelf);
        }

        private void OpenNewChartDialog()
        {
            newChartDialog.Open();
        }
    }
}