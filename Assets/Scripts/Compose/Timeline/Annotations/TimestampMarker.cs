using ArcCreate.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Timeline
{
    public class TimestampMarker : MonoBehaviour, IDragHandler
    {
        [SerializeField] private TMP_InputField text;
        [SerializeField] private RectTransform textArea;
        [SerializeField] private RectTransform background;
        [SerializeField] private float spacing;
        [SerializeField] private double unfocusDelay = 0.5;

        private RectTransform rectTransform;
        private RectTransform parentRectTransform;
        private RectTransform[] textRectTransform = new RectTransform[0];
        private Timestamp timestamp;
        private double lastFocusedTime;

        public Timestamp Timestamp => timestamp;

        /// <summary>
        /// Gets or sets the current audio timing value of this marker.
        /// </summary>
        public int AudioTiming
        {
            get => timestamp.Timing;
            set
            {
                timestamp.Timing = value;
                if (gameObject.activeInHierarchy)
                {
                    UpdatePosition();
                }
            }
        }

        public bool IsFocused => Time.realtimeSinceStartup <= lastFocusedTime + unfocusDelay;

        public void SetContent(Timestamp timestamp)
        {
            this.timestamp = timestamp;
            AudioTiming = timestamp.Timing;
            text.text = timestamp.Message ?? string.Empty;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Values.LockTimestampEditing)
            {
                return;
            }

            Vector2 screenPos = eventData.position;
            screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);

            float parentWidth = parentRectTransform.rect.width;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRectTransform,
                screenPos,
                eventData.pressEventCamera,
                out Vector2 local);
            local.x = Mathf.Clamp(local.x, -parentWidth, parentWidth);
            SetDragPosition(local.x);
        }

        private void Awake()
        {
            text.onValueChanged.AddListener(OnTextChange);
            rectTransform = GetComponent<RectTransform>();
            parentRectTransform = transform.parent.GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            text.onValueChanged.RemoveListener(OnTextChange);
        }

        private void OnTextChange(string txt)
        {
            timestamp.Message = string.IsNullOrWhiteSpace(txt) ? null : txt;
            textRectTransform = textArea.GetComponentsInChildren<RectTransform>();
        }

        private void SetDragPosition(float x)
        {
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);

            float parentWidth = parentRectTransform.rect.width / 2;

            int viewFrom = Services.Timeline.ViewFromTiming;
            int viewTo = Services.Timeline.ViewToTiming;
            float p = (x + parentWidth) / (parentWidth * 2);

            AudioTiming = Mathf.RoundToInt(Mathf.Lerp(viewFrom, viewTo, p));
            timestamp.Timing = AudioTiming;
            AlignNumberBackground();
        }

        private void AlignNumberBackground()
        {
            rectTransform.sizeDelta = new Vector2(text.preferredWidth + spacing, background.sizeDelta.y);
            for (int i = 0; i < textRectTransform.Length; i++)
            {
                RectTransform rect = textRectTransform[i];
                rect.anchoredPosition = Vector2.zero;
            }

            float numWidth = background.rect.width / 2;
            float parentWidth = parentRectTransform.rect.width / 2;
            float x = rectTransform.anchoredPosition.x;

            if (x <= -parentWidth + numWidth)
            {
                background.anchoredPosition = new Vector2(-parentWidth + numWidth - x, background.anchoredPosition.y);
            }
            else if (x >= parentWidth - numWidth)
            {
                background.anchoredPosition = new Vector2(parentWidth - numWidth - x, background.anchoredPosition.y);
            }
            else
            {
                background.anchoredPosition = new Vector2(0, background.anchoredPosition.y);
            }
        }

        private void Update()
        {
            UpdatePosition();
            if (text.isFocused)
            {
                lastFocusedTime = Time.realtimeSinceStartup;
            }
        }

        private void UpdatePosition()
        {
            int viewFrom = Services.Timeline.ViewFromTiming;
            int viewTo = Services.Timeline.ViewToTiming;
            float p = (float)(AudioTiming - viewFrom) / (viewTo - viewFrom);

            float parentWidth = parentRectTransform.rect.width / 2;
            float x = Mathf.Lerp(-parentWidth, parentWidth, p);
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
            AlignNumberBackground();
        }
    }
}