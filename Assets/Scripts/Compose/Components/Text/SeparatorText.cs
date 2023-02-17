using UnityEngine;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Component for text in a separator object.
    /// </summary>
    public class SeparatorText : I18nText
    {
        [SerializeField] private RectTransform divideLine;
        [SerializeField] private float spacing;

        public override void ApplyLocale()
        {
            base.ApplyLocale();

            float textWidth = Text.preferredWidth;
            divideLine.offsetMax = new Vector2(
                -textWidth - spacing,
                divideLine.offsetMax.y);
        }
    }
}