using ArcCreate.Compose.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Project
{
    [RequireComponent(typeof(RectTransform))]
    public class LineHighlightComponent : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Color[] colors;

        private RectTransform rect;
        private string text;
        private Severity severity;

        public void SetPosition(TMP_InputField inputField, Option<int> lineNumber, Option<int> startCharPos, Option<int> length)
        {
            if (rect == null)
            {
                rect = GetComponent<RectTransform>();
            }

            int lineNumVal = lineNumber.Or(0);
            TMP_LineInfo[] lineInfoArray = inputField.textComponent.textInfo.lineInfo;
            lineNumber = Mathf.Clamp(lineNumVal - 1, 0, lineInfoArray.Length - 1);
            TMP_LineInfo lineInfo = lineInfoArray[lineNumVal];

            float ascender = lineInfo.ascender;
            float descender = lineInfo.descender;

            if (!startCharPos.HasValue)
            {
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                rect.offsetMin = new Vector2(0, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0, rect.offsetMax.y);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, ascender);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(ascender - descender));
            }
            else
            {
                TMP_CharacterInfo[] charInfoArray = inputField.textComponent.textInfo.characterInfo;
                int leftIndex = lineInfo.firstCharacterIndex + startCharPos.Value;
                int rightIndex = length.HasValue ? (lineInfo.firstCharacterIndex + length.Value) : lineInfo.lastCharacterIndex;
                float left = (leftIndex >= 0 && leftIndex < charInfoArray.Length) ? charInfoArray[leftIndex].topLeft.x : 0;
                float right = (rightIndex >= 0 && rightIndex < charInfoArray.Length) ? charInfoArray[rightIndex].topRight.x : 0;

                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2((left + right) / 2, ascender);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(ascender - descender));
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