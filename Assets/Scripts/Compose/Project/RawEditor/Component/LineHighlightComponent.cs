using System.Collections.Generic;
using ArcCreate.Compose.Popups;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    [RequireComponent(typeof(RectTransform))]
    public class LineHighlightComponent : MonoBehaviour, IPointerEnterHandler
    {
        private const float MinWidth = 10;

        [SerializeField] private Image background;
        [SerializeField] private Color[] colors;

        private RectTransform rect;
        private string text;
        private Severity severity;

        public void SetPosition(TextGenerator gen, Option<int> lineNumber, Option<int> startCharPos, Option<int> length)
        {
            if (rect == null)
            {
                rect = GetComponent<RectTransform>();
            }

            int lineNumVal = lineNumber.Or(0);
            IList<UILineInfo> lineInfoArray = gen.lines;
            if (lineInfoArray.Count <= 0)
            {
                return;
            }

            lineNumVal = Mathf.Clamp(lineNumVal, 0, lineInfoArray.Count - 1);
            UILineInfo lineInfo = lineInfoArray[lineNumVal];
            UILineInfo firstLineInfo = lineInfoArray[0];

            float ascender = firstLineInfo.topY - lineInfo.topY;

            if (!startCharPos.HasValue)
            {
                rect.anchorMin = new Vector2(0, 1f);
                rect.anchorMax = new Vector2(1, 1f);
                rect.offsetMin = new Vector2(0, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0, rect.offsetMax.y);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -ascender);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, lineInfo.height);
            }
            else
            {
                IList<UICharInfo> charInfoArray = gen.characters;

                int nextLineStartIdx = gen.characterCount - 1;
                if (lineNumVal + 1 < gen.lineCount)
                {
                    nextLineStartIdx = lineInfoArray[lineNumVal + 1].startCharIdx - 1;
                }

                int leftIndex = lineInfo.startCharIdx + startCharPos.Value;
                int rightIndex = (length.HasValue && length.Value > 0) ? (leftIndex + length.Value - 1) : nextLineStartIdx;
                rightIndex = Mathf.Max(leftIndex + 1, rightIndex);

                float left = (leftIndex >= 0 && leftIndex < charInfoArray.Count) ? charInfoArray[leftIndex].cursorPos.x : 0;
                float right = (rightIndex >= 0 && rightIndex < charInfoArray.Count) ? charInfoArray[rightIndex].cursorPos.x + charInfoArray[rightIndex].charWidth : left + MinWidth;

                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2((left + right) / 2, -ascender);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, lineInfo.height);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs(right - left));
            }
        }

        public void SetContent(Severity severity, string text)
        {
            this.text = text;
            this.severity = severity;
            background.color = colors[(int)severity];
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Services.Popups.ShowHint(eventData.position, severity, text, rect, eventData.enterEventCamera);
        }
    }
}