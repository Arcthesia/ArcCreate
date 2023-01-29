using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Popups
{
    [RequireComponent(typeof(RectTransform))]
    public class TextDialog : MonoBehaviour
    {
        private RectTransform rect;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private RectTransform contentRect;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform buttonParent;
        [SerializeField] private float extraHeight;
        [SerializeField] private float maxHeight;
        [SerializeField] private Color[] buttonColors;

        public void Setup(string title, string content, ButtonSetting[] buttonSettings)
        {
            titleText.text = title;
            contentText.text = content;

            foreach (ButtonSetting setting in buttonSettings)
            {
                GameObject go = Instantiate(buttonPrefab, buttonParent);
                TextDialogButton button = go.GetComponent<TextDialogButton>();
                Color color = buttonColors[(int)setting.ButtonColor];
                button.Setup(setting.Text, setting.Callback, color, this);
            }

            SetBoxHeightCoroutine().Forget();
        }

        public void CloseSelf()
        {
            Destroy(gameObject);
        }

        // I hate unity so much
        private async UniTask SetBoxHeightCoroutine()
        {
            await UniTask.NextFrame();

            float preferredHeight = contentRect.rect.height;
            rect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                Mathf.Min(maxHeight, preferredHeight + extraHeight));
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }
    }
}