using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    public class NewChartDialog : MonoBehaviour
    {
        [SerializeField] private TMP_InputField chartNameField;
        [SerializeField] private TMP_Dropdown chartExtension;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject warningObject;
        [SerializeField] private TMP_Text warningText;

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void OnConfirm(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                warningObject.SetActive(true);
                warningText.text = I18n.S("Compose.Exception.InvalidChartFile");
                return;
            }

            string fullName = name + chartExtension.options[chartExtension.value].text;
            foreach (var chart in Services.Project.CurrentProject.Charts)
            {
                if (chart.ChartPath == fullName)
                {
                    warningObject.SetActive(true);
                    warningText.text = I18n.S("Compose.Exception.ChartFileAlreadyExists");
                    return;
                }
            }

            warningObject.SetActive(false);
            chartNameField.text = string.Empty;
            Close();
            Services.Project.CreateNewChart(fullName);
        }

        private void OnConfirmButton()
        {
            OnConfirm(chartNameField.text);
        }

        private void Awake()
        {
            chartNameField.text = string.Empty;
            chartNameField.onEndEdit.AddListener(OnConfirm);
            confirmButton.onClick.AddListener(OnConfirmButton);
            closeButton.onClick.AddListener(Close);
            warningObject.SetActive(false);
        }

        private void OnDestroy()
        {
            chartNameField.onEndEdit.RemoveListener(OnConfirm);
            confirmButton.onClick.RemoveListener(OnConfirmButton);
            closeButton.onClick.RemoveListener(Close);
        }
    }
}