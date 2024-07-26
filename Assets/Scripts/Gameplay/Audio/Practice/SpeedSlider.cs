using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Gameplay.Audio.Practice
{
    public class SpeedSlider : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        public const int DefaultValue = 1;
        public const float MaxValue = 2;
        public const float MinValue = 0;
        public const float AbsoluteMin = 0.01f;
        public const float Increment = 0.05f;
        public const float IncrementLarge = 0.25f;
        public const float ClickDuration = 0.1f;

        [SerializeField] private RectTransform fillRect;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Button incrementButtton;
        [SerializeField] private Button decrementButtton;

        private float value = DefaultValue;
        private float lastDown = float.MinValue;
        private RectTransform rect;

        public event Action<float> OnValueChanged;

        public float Value
        {
            get => value;
            set { SetValue(value); }
        }

        public void OnDrag(PointerEventData ev)
        {
            if (Time.realtimeSinceStartup < lastDown + ClickDuration)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, ev.position, gameplayCamera, out Vector2 local);
            float pivot = Mathf.Clamp(local.x / rect.rect.width, -0.5f, 0.5f) + 0.5f;
            bool cursorOnRect = Mathf.Abs(local.y) <= rect.rect.height * 1.5f;
            float increment = cursorOnRect ? IncrementLarge : Increment;
            float realSpeed = Mathf.Lerp(MinValue, MaxValue, pivot);
            float snapped = Mathf.Round(realSpeed / increment) * increment;
            snapped = Mathf.Clamp(snapped, AbsoluteMin, MaxValue);

            SetValue(snapped);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            lastDown = Time.realtimeSinceStartup;
        }

        public void SetValue(float value)
        {
            SetValueWithoutNotify(value);
            OnValueChanged.Invoke(value);
        }

        public void SetValueWithoutNotify(float value)
        {
            this.value = value;
            UpdateUI();
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            incrementButtton.onClick.AddListener(IncrementSpeed);
            decrementButtton.onClick.AddListener(DecrementSpeed);
        }

        private void OnDestroy()
        {
            incrementButtton.onClick.RemoveListener(IncrementSpeed);
            decrementButtton.onClick.RemoveListener(DecrementSpeed);
        }

        private void IncrementSpeed()
        {
            if (Value <= AbsoluteMin)
            {
                SetValue(Increment);
            }

            float newValue = Mathf.Round(Value / Increment + 1) * Increment;
            SetValue(Mathf.Clamp(newValue, AbsoluteMin, MaxValue));
        }

        private void DecrementSpeed()
        {
            float newValue = Mathf.Round(Value / Increment - 1) * Increment;
            SetValue(Mathf.Clamp(newValue, AbsoluteMin, MaxValue));
        }

        private void UpdateUI()
        {
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(value / MaxValue, 1);
        }
    }
}