using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class ChartPickerRow : MonoBehaviour
    {
        [SerializeField] private Button mainButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private GameObject currentlyActiveIndicator;
        [SerializeField] private TMP_Text label;
        private ChartSettings chart;

        public void SetChart(ChartSettings chart)
        {
            this.chart = chart;
            label.text = chart.ChartPath;
        }

        public void SetCurrentlyActive(bool isCurrent)
        {
            removeButton.gameObject.SetActive(!isCurrent);
            currentlyActiveIndicator.SetActive(isCurrent);
        }

        private void Awake()
        {
            mainButton.onClick.AddListener(Select);
            removeButton.onClick.AddListener(Remove);
            Services.Project.OnChartLoad += OnChartLoad;
        }

        private void OnDestroy()
        {
            mainButton.onClick.RemoveListener(Select);
            removeButton.onClick.RemoveListener(Remove);
            Services.Project.OnChartLoad -= OnChartLoad;
        }

        private void Remove()
        {
            Services.Popups.CreateTextDialog(
                I18n.S("Compose.Dialog.RemoveChart.Title"),
                I18n.S("Compose.Dialog.RemoveChart.Content"),
                new Popups.ButtonSetting
                {
                    Text = I18n.S("Compose.Dialog.RemoveChart.Yes"),
                    Callback = OnRemoveConfirm,
                    ButtonColor = Popups.ButtonColor.Danger,
                },
                new Popups.ButtonSetting
                {
                    Text = I18n.S("Compose.Dialog.RemoveChart.No"),
                    Callback = null,
                    ButtonColor = Popups.ButtonColor.Default,
                });
        }

        private void OnRemoveConfirm()
        {
            Services.Project.RemoveChart(chart);
        }

        private void Select()
        {
            Services.Project.OpenChart(chart);
        }

        private void OnChartLoad(ChartSettings chart)
        {
            SetCurrentlyActive(this.chart == chart);
        }
    }
}