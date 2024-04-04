using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class SaveCustomPanelLayoutDialog : Dialog
    {
        [SerializeField] private Button confirm;
        [SerializeField] private Button cancel;
        [SerializeField] private TMP_InputField labelInput;
        [SerializeField] private TMP_Text duplicateError;
        private PanelLayoutManager manager;

        public void Open(PanelLayoutManager manager)
        {
            this.manager = manager;
            labelInput.text = string.Empty;
            Open();
        }

        private void Awake()
        {
            confirm.onClick.AddListener(OnConfirm);
            cancel.onClick.AddListener(OnCancel);
            labelInput.onValueChanged.AddListener(OnLabelInputChange);
        }

        private void OnLabelInputChange(string text)
        {
            if (!manager.IsValidLabel(text, out string reason))
            {
                duplicateError.gameObject.SetActive(true);
                duplicateError.text = reason;
            }
            else
            {
                duplicateError.gameObject.SetActive(false);
            }
        }

        private void OnCancel()
        {
            Close();
        }

        private void OnConfirm()
        {
            if (!manager.IsValidLabel(labelInput.text, out string _))
            {
                return;
            }

            manager.SaveLayout(labelInput.text);
            Close();
        }
    }
}