using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class NewChartDialog : MonoBehaviour
    {
        [SerializeField] private TMP_InputField chartNameField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private GameObject fileExistsWarning;

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(true);
        }

        private void OnConfirm(string name)
        {
            foreach (var chart in Services.Project.CurrentProject.Charts)
            {
                if (chart.ChartPath == name)
                {
                    fileExistsWarning.SetActive(true);
                    return;
                }
            }

            fileExistsWarning.SetActive(false);
            chartNameField.text = Strings.DefaultChartExtension;
            Services.Project.CreateNewChart(name);
        }

        private void OnConfirmButton()
        {
            OnConfirm(chartNameField.text);
        }

        private void Awake()
        {
            chartNameField.text = Strings.DefaultChartExtension;
            chartNameField.onEndEdit.AddListener(OnConfirm);
            confirmButton.onClick.AddListener(OnConfirmButton);
        }

        private void OnDestroy()
        {
            chartNameField.onEndEdit.RemoveListener(OnConfirm);
            confirmButton.onClick.RemoveListener(OnConfirmButton);
        }
    }
}