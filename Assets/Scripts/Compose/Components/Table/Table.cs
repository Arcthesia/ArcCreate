using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Table<T> : MonoBehaviour, IScrollHandler
    {
        private RectTransform rectTransform;
        [SerializeField] private GameObject rowPrefab;
        [SerializeField] private int maxRowCount = 50;
        [SerializeField] private Scrollbar verticalScrollbar;
        [SerializeField] private Scrollbar horizontalScrollbar;
        [SerializeField] private ColumnNamesRow columnNamesRow;
        [SerializeField] private List<Column> columns;
        [SerializeField] private int staticColumnsCount = 1;

        private float prevHeight;
        private Pool<Row<T>> rowPool;
        private readonly List<Row<T>> rows = new List<Row<T>>();
        private List<T> data = new List<T>();

        private int StartPos { get; set; } = 0;

        private int EndPos => StartPos + rows.Count;

        private float StaticWidth
        {
            get
            {
                float w = 0;
                for (int i = 0; i < Mathf.Min(columns.Count - 1, staticColumnsCount); i++)
                {
                    Column column = columns[i];
                    w += column.Size;
                }

                return w;
            }
        }

        private float TotalWidth
        {
            get
            {
                float w = 0;
                foreach (var column in columns)
                {
                    w += column.Size;
                }

                return Mathf.Max(rectTransform.rect.width, w - StaticWidth);
            }
        }

        public void SetColumns(List<Column> columns)
        {
            this.columns = columns;
            UpdateHorizontalScrollbar();
        }

        public void SetData(List<T> data)
        {
            this.data = data;
            BuildList();
        }

        public void OnScroll(PointerEventData eventData)
        {
            float delta = eventData.scrollDelta.y;
            int count = rows.Count;

            int oldStartPos = StartPos;
            StartPos += (int)Mathf.Sign(delta * Settings.ScrollSensitivityVertical.Value);
            if (StartPos < 0 || EndPos > count)
            {
                StartPos = oldStartPos;
            }

            UpdateVerticalScrollbar();
            BuildList();
        }

        protected void Update()
        {
            if (rectTransform.rect.height != prevHeight)
            {
                RebuildRows();
            }

            prevHeight = rectTransform.rect.height;
        }

        protected void RebuildRows()
        {
            rowPool.ReturnAll();
            rows.Clear();

            float currentHeight = 0;
            int count = 0;
            while (currentHeight <= rectTransform.rect.height)
            {
                Row<T> row = rowPool.Get();
                rows.Add(row);
                row.Table = this;

                row.RectTransform.anchoredPosition = new Vector2(
                    currentHeight,
                    row.RectTransform.anchoredPosition.y);
                currentHeight += row.RectTransform.rect.height;

                // Avoid crashing the operating system if the prefab's height is 0
                count++;
                if (count > maxRowCount)
                {
                    return;
                }
            }
        }

        protected void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rowPool = Pools.New<Row<T>>(rowPrefab.name, rowPrefab, transform, 1);
            verticalScrollbar.onValueChanged.AddListener(OnVerticalScrollbar);
            horizontalScrollbar.onValueChanged.AddListener(OnHorizontalScrollbar);
            RebuildRows();

            if (columns != null && columns.Count > 0)
            {
                columnNamesRow.SetReference(columns);
            }
        }

        protected void OnDestroy()
        {
            verticalScrollbar.onValueChanged.RemoveListener(OnVerticalScrollbar);
            horizontalScrollbar.onValueChanged.RemoveListener(OnHorizontalScrollbar);
        }

        private void BuildList()
        {
            int count = data.Count;
            int extracount = Mathf.Max(count - rows.Count, 0);
            float size = (float)rows.Count / (extracount + rows.Count);
            verticalScrollbar.size = size;

            int rowIdx = 0;
            int end = EndPos > count ? count : EndPos;
            for (int k = StartPos; k < end; ++k)
            {
                Row<T> row = rows[rowIdx++];
                T datum = data[k];
                row.SetReference(datum);
                row.SetInteractable(true);
            }

            // Disable unused
            for (int k = rowIdx; k < rows.Count; ++k)
            {
                Row<T> row = rows[k];
                row.RemoveReference();
                row.SetInteractable(false);
            }
        }

        private void OnVerticalScrollbar(float value)
        {
            int count = data.Count;
            int extracount = count - rows.Count;
            extracount = extracount < 0 ? 0 : extracount;
            float size = (float)rows.Count / (extracount + rows.Count);
            verticalScrollbar.size = size;

            float pos = verticalScrollbar.value;
            StartPos = Mathf.RoundToInt(pos * extracount);
            BuildList();
        }

        private void OnHorizontalScrollbar(float value)
        {
            float totalWidth = TotalWidth;
            float staticWidth = StaticWidth;
            float viewWidth = rectTransform.rect.width - staticWidth;

            float x = Mathf.Lerp(0, -totalWidth + viewWidth, horizontalScrollbar.value);
            foreach (var row in rows)
            {
                row.SetHorizontalScroll(x);
            }

            columnNamesRow.SetHorizontalScroll(x);
        }

        private void UpdateVerticalScrollbar()
        {
            int count = data.Count;
            int extracount = count - rows.Count;
            extracount = extracount < 0 ? 0 : extracount;
            float size = (float)rows.Count / (extracount + rows.Count);
            verticalScrollbar.size = size;

            if (extracount <= 0)
            {
                verticalScrollbar.value = 0;
            }
            else
            {
                verticalScrollbar.value = (float)StartPos / extracount;
            }
        }

        private void UpdateHorizontalScrollbar()
        {
            float totalWidth = TotalWidth;
            float staticWidth = StaticWidth;
            float viewWidth = rectTransform.rect.width - staticWidth;

            horizontalScrollbar.size = viewWidth / totalWidth;

            // Will also trigger OnHorizontalScrollbar
            horizontalScrollbar.value = 0;
        }
    }
}