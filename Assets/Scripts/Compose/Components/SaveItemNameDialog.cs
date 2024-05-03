using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class SaveItemNameDialog : Dialog
    {
        [SerializeField] private Button confirm;
        [SerializeField] private Button cancel;
        [SerializeField] private TMP_InputField labelInput;
        [SerializeField] private TMP_Text duplicateError;
        private IItemNameDialogConsumer consumer;

        public void Open(IItemNameDialogConsumer consumer)
        {
            this.consumer = consumer;
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
            if (!consumer.IsValidName(text, out string reason))
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
            if (!consumer.IsValidName(labelInput.text, out string _))
            {
                return;
            }

            consumer.SaveItem(labelInput.text);
            Close();
        }
    }
}