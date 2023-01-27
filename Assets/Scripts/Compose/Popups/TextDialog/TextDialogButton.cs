using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Popups
{
    public class TextDialogButton : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text contentText;
        private Action callback;
        private TextDialog dialog;

        public void Setup(string text, Action callback, Color color, TextDialog textDialog)
        {
            contentText.text = text;
            this.callback = callback;
            button.onClick.AddListener(OnClick);
            image.color = color;
            dialog = textDialog;
        }

        private void OnClick()
        {
            callback?.Invoke();
            dialog.CloseSelf();
        }
    }
}