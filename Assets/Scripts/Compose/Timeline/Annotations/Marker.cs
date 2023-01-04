using System;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Timeline
{
    public class Marker : MonoBehaviour, IDragHandler
    {
        [SerializeField] private TMP_InputField timingField;
        [SerializeField] private RectTransform numberBackground;
        [SerializeField] private float spacing;
        private RectTransform rectTransform;
        private RectTransform parentRectTransform;
        private bool queueTimingEdit = false;

        public event Action<Marker, int> OnValueChanged;

        public int Timing { get; private set; }

        public void OnDrag(PointerEventData eventData)
        {
            float parentWidth = parentRectTransform.rect.width / 2;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.enterEventCamera, out Vector2 local);
            local.x = Mathf.Clamp(local.x, -parentWidth, parentWidth);

            SetDragPosition(local.x);
        }

        public void SetTiming(int timing)
        {
            Timing = timing;
            SetFieldText(timing);
        }

        public void SetDragPosition(float x)
        {
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);

            float parentWidth = parentRectTransform.rect.width / 2;

            int viewFrom = Services.Timeline.ViewFromTiming;
            int viewTo = Services.Timeline.ViewToTiming;
            float p = (x + parentWidth) / (parentWidth * 2);

            Timing = Mathf.RoundToInt(Mathf.Lerp(viewFrom, viewTo, p));
            SetFieldText(Timing);
            OnValueChanged?.Invoke(this, Timing);

            AlignNumberBackground();
        }

        private void AlignNumberBackground()
        {
            rectTransform.sizeDelta = new Vector2(timingField.preferredWidth + spacing, numberBackground.sizeDelta.y);
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
                Timing = Mathf.Clamp(timing, 0, Services.Gameplay.Audio.AudioLength);
                OnValueChanged.Invoke(this, Timing);
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
            int viewFrom = Services.Timeline.ViewFromTiming;
            int viewTo = Services.Timeline.ViewToTiming;
            float p = (float)(Timing - viewFrom) / (viewTo - viewFrom);

            float parentWidth = parentRectTransform.rect.width / 2;
            float x = Mathf.Lerp(-parentWidth, parentWidth, p);
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
            AlignNumberBackground();

            if (!timingField.isFocused && queueTimingEdit)
            {
                timingField.text = Timing.ToString();
                queueTimingEdit = false;
            }
        }
    }
}