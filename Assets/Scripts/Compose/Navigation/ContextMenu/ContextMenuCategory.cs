using TMPro;
using UnityEngine;

namespace ArcCreate.Compose.Navigation
{
    public class ContextMenuCategory : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float baseHeight;
        [SerializeField] private RectTransform rect;

        public float BaseHeight => baseHeight;

        public void ResetSize()
        {
            rect.sizeDelta = new Vector2(0, BaseHeight);
        }

        public void ConfigureButton(ContextMenuButton button)
        {
            button.Rect.anchoredPosition = new Vector2(rect.rect.height, 0);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.rect.height + button.Rect.rect.height);
        }

        public void SetText(string i18nName)
        {
            text.text = I18n.S(i18nName);
        }
    }
}