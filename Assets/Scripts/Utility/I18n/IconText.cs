using UnityEngine;

namespace ArcCreate.Utility
{
    /// <summary>
    /// Component for resizing a rect transform to fit an icon and a text component.
    /// </summary>
    public class IconText : I18nText
    {
        [SerializeField] private RectTransform parent;
        [SerializeField] private float spacing;

        public override void ApplyLocale()
        {
            base.ApplyLocale();
            UpdateSize();
        }

        public void UpdateSize()
        {
            float textWidth = Text.preferredWidth;
            parent.sizeDelta = new Vector2(
                textWidth + spacing,
                parent.sizeDelta.y);
        }
    }
}