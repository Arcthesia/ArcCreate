using ArcCreate.Compose.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    [RequireComponent(typeof(RectTransform))]
    public class ScrollHighlightComponent : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Color[] colors;
        private RectTransform rect;

        public void SetPosition(TextGenerator gen, Option<int> lineNumber)
        {
            if (rect == null)
            {
                rect = GetComponent<RectTransform>();
            }

            int lineCount = gen.lineCount;
            int lineNumVal = lineNumber.Or(0);
            lineNumber = Mathf.Clamp(lineNumVal, 1, lineCount);
            float ratio = (float)lineNumVal / lineCount;

            rect.anchorMin = new Vector2(rect.anchorMin.x, 1 - ratio);
            rect.anchorMax = new Vector2(rect.anchorMax.x, 1 - ratio);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0);
        }

        public void SetSeverity(Severity severity)
        {
            image.color = colors[(int)severity];
        }
    }
}