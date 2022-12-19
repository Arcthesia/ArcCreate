using UnityEngine;

namespace ArcCreate.Compose.Components
{
    [ExecuteInEditMode]
    public class IconText : I18nText
    {
        [SerializeField] private RectTransform parent;
        [SerializeField] private float spacing;

        public override void ApplyLocale()
        {
            base.ApplyLocale();

            float textWidth = Text.preferredWidth;
            parent.sizeDelta = new Vector2(
                textWidth + spacing,
                parent.sizeDelta.y);
        }
    }
}