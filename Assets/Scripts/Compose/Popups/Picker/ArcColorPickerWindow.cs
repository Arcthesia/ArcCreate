using System;
using ArcCreate.Compose.Components;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Compose.Popups
{
    public class ArcColorPickerWindow : Table<ColorSetting>
    {
        [SerializeField] private GameObject window;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private float minDistanceFromBorder;
        private RectTransform rect;

        public Action<int> OnColorChanged { get; set; }

        public object Owner { get; private set; }

        public int Color
        {
            get => Selected.Id;
            set
            {
                int clamped = Mathf.Clamp(value, 0, Data.Count - 1);
                Selected = Data[clamped];
                OnColorChanged?.Invoke(clamped);
            }
        }

        public override ColorSetting Selected
        {
            get => base.Selected;
            set
            {
                base.Selected = value;
                OnColorChanged?.Invoke(value.Id);
                CloseWindow();
            }
        }

        public void OpenAt(Vector2 screenPosition, int? setColor, object caller)
        {
            window.SetActive(true);
            Owner = caller;

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

            UpdateTable();
            if (setColor != null)
            {
                base.Selected = Data[Mathf.Clamp(setColor.Value, 0, Data.Count - 1)];
            }
        }

        public void SetColorWithoutNotify(int value)
        {
            int clamped = Mathf.Clamp(value, 0, Data.Count - 1);
            base.Selected = Data[clamped];
        }

        protected override void Awake()
        {
            base.Awake();
            rect = window.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void UpdateRowHighlight()
        {
            foreach (var row in Rows)
            {
                row.Highlighted = row.Reference.Equals(Selected);
            }
        }

        private void CloseWindow()
        {
            window.SetActive(false);
        }

        private void UpdateTable()
        {
            var chart = Services.Project.CurrentChart;
            int colorCount = chart.Colors.Arc.Count;

            Data.Clear();
            for (int i = 0; i < colorCount; i++)
            {
                chart.Colors.Arc[Mathf.Clamp(i, 0, chart.Colors.Arc.Count - 1)].ConvertHexToColor(out Color high);
                chart.Colors.ArcLow[Mathf.Clamp(i, 0, chart.Colors.ArcLow.Count - 1)].ConvertHexToColor(out Color low);

                Data.Add(new ColorSetting
                {
                    Id = i,
                    High = high,
                    Low = low,
                });
            }

            SetData(Data);
        }
    }
}