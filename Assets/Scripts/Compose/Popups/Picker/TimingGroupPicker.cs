using System;
using System.Linq;
using ArcCreate.Compose.Components;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Popups
{
    public class TimingGroupPicker : Table<TimingGroup>
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject window;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private float minDistanceFromBorder;
        [SerializeField] private TMP_InputField numberField;
        private RectTransform rect;

        public Action<TimingGroup> OnValueChanged { get; set; }

        public Action<TimingGroup> OnEndEdit { get; set; }

        public TimingGroup Value { get; private set; }

        public object Owner { get; private set; }

        public void OpenAt(Vector2 screenPosition, int? defaultTg, object caller)
        {
            window.SetActive(true);
            Owner = caller;
            closeButton.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(gameObject);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out Vector2 position);

            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;
            float rectWidth = rect.rect.width;
            float rectHeight = rect.rect.height;
            Vector2 pivot = rect.pivot;

            float x = Mathf.Clamp(
                position.x,
                -(canvasWidth / 2) + minDistanceFromBorder + (rectWidth * pivot.x),
                (canvasWidth / 2) - minDistanceFromBorder - (rectWidth * (1 - pivot.x)));

            float y = Mathf.Clamp(
                position.y,
                -(canvasHeight / 2) + minDistanceFromBorder + (rectHeight * pivot.y),
                (canvasHeight / 2) - minDistanceFromBorder - (rectHeight * (1 - pivot.y)));

            rect.anchoredPosition = new Vector2(x, y);

            if (defaultTg != null)
            {
                TimingGroup tg = Services.Gameplay.Chart.GetTimingGroup(defaultTg.Value);
                if (!tg.GroupProperties.Editable)
                {
                    tg = Services.Gameplay.Chart.GetTimingGroup(0);
                }

                SetValueWithoutNotify(tg);
            }

            EventSystem.current.SetSelectedGameObject(numberField.gameObject);
        }

        public void SetValue(TimingGroup value)
        {
            SetValueWithoutNotify(value);
            OnValueChanged?.Invoke(Selected);
            OnEndEdit?.Invoke(Selected);
        }

        public void SetValueWithoutNotify(TimingGroup value)
        {
            Value = value;
            numberField.text = value.GroupNumber.ToString();
            BuildTable(numberField.text);
        }

        public void CloseWindow()
        {
            window.SetActive(false);
        }

        protected override void Awake()
        {
            base.Awake();
            numberField.onValueChanged.AddListener(OnNumberFieldChange);
            numberField.onSubmit.AddListener(OnNumberFieldSubmit);
            rect = window.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            closeButton.onClick.AddListener(CloseWindow);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            numberField.onValueChanged.RemoveListener(OnNumberFieldChange);
            numberField.onSubmit.RemoveListener(OnNumberFieldSubmit);
            closeButton.onClick.RemoveListener(CloseWindow);
        }

        private void OnNumberFieldChange(string val)
        {
            BuildTable(val);
            if (Selected != null)
            {
                OnValueChanged?.Invoke(Selected);
            }
        }

        private void OnNumberFieldSubmit(string val)
        {
            CloseWindow();
            if (Selected != null)
            {
                OnEndEdit?.Invoke(Selected);
            }
        }

        private void BuildTable(string query)
        {
            if (Evaluator.TryInt(query, out int group))
            {
                SetData(Services.Gameplay.Chart.TimingGroups.Where(tg => tg.GroupProperties.Editable).ToList());
                foreach (var timingGroup in Data)
                {
                    if (timingGroup.GroupNumber == group)
                    {
                        Selected = timingGroup;
                        Value = Selected;
                    }
                }
            }
            else
            {
                SetData(Services.Gameplay.Chart.TimingGroups
                    .Where(tg => tg.GroupProperties.Editable
                                    && tg.GroupProperties.Name != null
                                    && tg.GroupProperties.Name.Contains(query)).ToList());
            }

            if (Data.Contains(Value))
            {
                Selected = Value;
            }
            else
            {
                Selected = Data.Count > 0 ? Data[0] : null;
                Value = Selected;
            }

            if (Selected != null)
            {
                JumpTo(IndexOf(Selected));
            }
        }
    }
}