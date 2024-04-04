using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ArcCreate.Compose.Rendering
{
    public class CrfField : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        public const int DefaultValue = 23;
        public const int MaxValue = 51;
        public const int MinValue = 0;
        public const float ClickDuration = 0.1f;

        [SerializeField] private Camera editorCamera;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text text;
        [SerializeField] private RectTransform fillRect;

        private int value = DefaultValue;
        private RectTransform rect;
        private float lastDown = float.MinValue;

        public int Value
        {
            get => value;
            set { SetValue(value); }
        }

        public CrfEvent OnValueChanged { get; private set; } = new CrfEvent();

        public void OnDrag(PointerEventData ev)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, ev.position, editorCamera, out Vector2 local);
            float pivot = Mathf.Clamp(local.x / rect.rect.width, -0.5f, 0.5f) + 0.5f;
            SetValue(Mathf.RoundToInt(pivot * MaxValue));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (lastDown + ClickDuration < Time.realtimeSinceStartup)
            {
                return;
            }

            inputField.SetTextWithoutNotify(value.ToString());
            inputField.gameObject.SetActive(true);
            text.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            lastDown = Time.realtimeSinceStartup;
        }

        public void SetValue(int value)
        {
            SetValueWithoutNotify(value);
            OnValueChanged.Invoke(value);
        }

        public void SetValueWithoutNotify(int value)
        {
            this.value = value;
            UpdateUI();
        }

        private void UpdateUI()
        {
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2((float)value / MaxValue, 1);
            string i18n = value == MinValue ? "Lossless" :
                          value == DefaultValue ? "Default" :
                          value <= 18 ? "VeryLow" :
                          value <= 28 ? "Low" :
                          value <= 38 ? "Medium" :
                          "High";
            string s = I18n.S($"Compose.UI.Export.Render.Settings.CRF.{i18n}", value);
            text.SetText(s);
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            inputField.onDeselect.AddListener(DisableInputField);
            inputField.onEndEdit.AddListener(OnEndEdit);
            inputField.onSubmit.AddListener(OnEndEdit);
        }

        private void OnDestroy()
        {
            inputField.onDeselect.RemoveListener(DisableInputField);
            inputField.onEndEdit.RemoveListener(OnEndEdit);
            inputField.onSubmit.RemoveListener(OnEndEdit);
        }

        private void OnEndEdit(string s)
        {
            if (Evaluator.TryInt(s, out int crf))
            {
                crf = Mathf.Clamp(crf, MinValue, MaxValue);
                SetValue(crf);
            }

            inputField.gameObject.SetActive(false);
            text.gameObject.SetActive(true);
        }

        private void DisableInputField(string s)
        {
            inputField.gameObject.SetActive(false);
            text.gameObject.SetActive(true);
        }

        public class CrfEvent : UnityEvent<int>
        {
        }
    }
}