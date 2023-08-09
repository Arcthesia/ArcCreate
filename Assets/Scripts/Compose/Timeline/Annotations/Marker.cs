using System;
using ArcCreate.Utility.Parser;
using Cysharp.Threading.Tasks.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Timeline
{
    /// <summary>
    /// Component for markers displayed on the timeline.
    /// </summary>
    public class Marker : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TMP_InputField timingField;
        [SerializeField] private RectTransform textArea;
        [SerializeField] private RectTransform numberBackground;
        [SerializeField] private float spacing;
        [SerializeField] private bool useChartTiming;

        private RectTransform rectTransform;
        private RectTransform parentRectTransform;
        private RectTransform[] textRectTransform = new RectTransform[0];
        private bool queueTimingEdit = false;
        private int previousOffset;

        /// <summary>
        /// Invoked whenever the timing value of this marker changes.
        /// </summary>
        public event Action<Marker, int> OnValueChanged;

        /// <summary>
        /// Invoked after user finishes dragging the marker, or after editing the timing input field.
        /// </summary>
        public event Action<Marker, int> OnEndEdit;

        /// <summary>
        /// Gets the current audio timing value of this marker.
        /// </summary>
        public int AudioTiming { get; private set; }

        /// <summary>
        /// Gets the current chart timing value of this marker.
        /// </summary>
        public int ChartTiming => AudioTiming - Gameplay.Values.ChartAudioOffset;

        /// <summary>
        /// Gets a value indicating whether or not this dragger is being dragged.
        /// </summary>
        public bool IsDragging { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not this marker uses chart timing instead of audio timing.
        /// </summary>
        public bool UseChartTiming => useChartTiming;

        public void OnDrag(PointerEventData eventData)
        {
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
            IsDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndEdit?.Invoke(this, useChartTiming ? ChartTiming : AudioTiming);
            IsDragging = false;
        }

        /// <summary>
        /// Set the marker's timing and update its position.
        /// </summary>
        /// <param name="timing">The timing value. Pass chart timing if <see cref="UseChartTiming"/> is true.</param>
        public void SetTiming(int timing)
        {
            AudioTiming = timing + (useChartTiming ? Gameplay.Values.ChartAudioOffset : 0);
            SetFieldText(AudioTiming);
            if (gameObject.activeInHierarchy)
            {
                UpdatePosition();
            }

            textRectTransform = textArea.GetComponentsInChildren<RectTransform>();
        }

        public void SetDragPosition(float x)
        {
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);

            float parentWidth = parentRectTransform.rect.width / 2;

            int viewFrom = Services.Timeline.ViewFromTiming;
            int viewTo = Services.Timeline.ViewToTiming;
            float p = (x + parentWidth) / (parentWidth * 2);

            AudioTiming = Mathf.RoundToInt(Mathf.Lerp(viewFrom, viewTo, p));
            SetFieldText(AudioTiming);
            OnValueChanged?.Invoke(this, useChartTiming ? ChartTiming : AudioTiming);

            AlignNumberBackground();
        }

        private void AlignNumberBackground()
        {
            rectTransform.sizeDelta = new Vector2(timingField.preferredWidth + spacing, numberBackground.sizeDelta.y);
            for (int i = 0; i < textRectTransform.Length; i++)
            {
                RectTransform rect = textRectTransform[i];
                rect.anchoredPosition = Vector2.zero;
            }

            float numWidth = numberBackground.rect.width / 2;
            float parentWidth = parentRectTransform.rect.width / 2;
            float x = rectTransform.anchoredPosition.x;

            if (x <= -parentWidth + numWidth)
            {
                numberBackground.anchoredPosition = new Vector2(-parentWidth + numWidth - x, numberBackground.anchoredPosition.y);
            }
            else if (x >= parentWidth - numWidth)
            {
                numberBackground.anchoredPosition = new Vector2(parentWidth - numWidth - x, numberBackground.anchoredPosition.y);
            }
            else
            {
                numberBackground.anchoredPosition = new Vector2(0, numberBackground.anchoredPosition.y);
            }
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            timingField.onEndEdit.AddListener(OnTimingField);
            parentRectTransform = transform.parent.GetComponent<RectTransform>();
            textRectTransform = textArea.GetComponentsInChildren<RectTransform>();
        }

        private void OnDestroy()
        {
            timingField.onEndEdit.RemoveListener(OnTimingField);
        }

        private void OnTimingField(string val)
        {
            if (Evaluator.TryFloat(val, out float num))
            {
                int timing = Mathf.RoundToInt(num);
                AudioTiming = Mathf.Clamp(timing, 0, Services.Gameplay.Audio.AudioLength);
                OnValueChanged?.Invoke(this, useChartTiming ? ChartTiming : AudioTiming);
                OnEndEdit?.Invoke(this, useChartTiming ? ChartTiming : AudioTiming);
            }

            timingField.text = num.ToString();
        }

        private void SetFieldText(int timing)
        {
            if (!timingField.isFocused)
            {
                timingField.text = timing.ToString();
            }
            else
            {
                queueTimingEdit = true;
            }
        }

        private void Update()
        {
            UpdatePosition();
            if ((!timingField.isFocused && queueTimingEdit) || (Gameplay.Values.ChartAudioOffset != previousOffset))
            {
                if (useChartTiming)
                {
                    int chartTiming = AudioTiming - previousOffset;
                    AudioTiming = chartTiming + Gameplay.Values.ChartAudioOffset;
                }

                int timing = useChartTiming ? ChartTiming : AudioTiming;
                timingField.text = AudioTiming.ToString();
                queueTimingEdit = false;
            }

            previousOffset = Gameplay.Values.ChartAudioOffset;
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