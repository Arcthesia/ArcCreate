using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Table<T> : MonoBehaviour, IScrollHandler
    {
        [SerializeField] private GameObject rowPrefab;
        [SerializeField] private RectTransform rowParent;
        [SerializeField] private int maxRowCount = 50;
        [SerializeField] private Scrollbar verticalScrollbar;
        [SerializeField] private ScrollReceiver scrollReceiver;
        private T selected;

        private float prevHeight;
        private Pool<Row<T>> rowPool;
        private readonly List<Row<T>> rows = new List<Row<T>>();
        private List<T> data = new List<T>();
        private float rowHeight;

        public virtual T Selected
        {
            get => selected;
            set
            {
                selected = value;
                if (value != null)
                {
                    UpdateRowHighlight();
                }
            }
        }

        protected List<Row<T>> Rows => rows;

        protected List<T> Data => data;

        protected int StartPos { get; set; } = 0;

        protected int EndPos => StartPos + rows.Count;

        public void SetData(List<T> data)
        {
            this.data = data;
            ClampStartPos();
            BuildList();
        }

        public void JumpTo(int index)
        {
            while (index < StartPos && StartPos > 0)
            {
                StartPos -= 1;
            }

            while (index >= EndPos && EndPos < Data.Count)
            {
                StartPos += 1;
            }

            ClampStartPos();
            UpdateVerticalScrollbar();
            BuildList();
        }

        public int IndexOf(T datum)
        {
            for (int i = 0; i < data.Count; i++)
            {
                T t = data[i];
                if (ReferenceEquals(t, datum))
                {
                    return i;
                }
            }

            return default;
        }

        public void OnScroll(PointerEventData eventData)
        {
            float delta = eventData.scrollDelta.y;
            int count = rows.Count;

            StartPos -= (int)Mathf.Sign(delta * Settings.ScrollSensitivityVertical.Value);
            ClampStartPos();
            UpdateVerticalScrollbar();
            BuildList();
            UnfocusRowFields();
        }

        protected virtual void Update()
        {
            if (rowParent.rect.height != prevHeight)
            {
                RebuildRows();
            }

            prevHeight = rowParent.rect.height;
        }

        protected void RebuildRows()
        {
            rowPool.ReturnAll();
            rows.Clear();

            float currentHeight = 0;
            int count = 0;
            while (currentHeight + rowHeight <= rowParent.rect.height)
            {
                Row<T> row = rowPool.Get();
                rows.Add(row);
                row.Table = this;

                row.RectTransform.anchoredPosition = new Vector2(
                    row.RectTransform.anchoredPosition.x,
                    -currentHeight);
                currentHeight += row.RectTransform.rect.height;

                // Avoid crashing the operating system if the prefab's height is 0
                count++;
                if (count > maxRowCount)
                {
                    return;
                }
            }

            ClampStartPos();
            BuildList();
        }

        protected virtual void Awake()
        {
            rowPool = Pools.New<Row<T>>(rowPrefab.name, rowPrefab, rowParent, 1);
            verticalScrollbar.onValueChanged.AddListener(OnVerticalScrollbar);
            scrollReceiver.OnScroll += OnScroll;
            RebuildRows();

            rowHeight = rowPrefab.GetComponent<RectTransform>().rect.height;
        }

        protected virtual void OnDestroy()
        {
            verticalScrollbar.onValueChanged.RemoveListener(OnVerticalScrollbar);
            scrollReceiver.OnScroll -= OnScroll;
            Pools.Destroy<Row<T>>(rowPrefab.name);
        }

        private void BuildList()
        {
            int count = data.Count;
            int extracount = Mathf.Max(count - rows.Count, 0);
            float size = (float)rows.Count / (extracount + rows.Count);
            verticalScrollbar.size = size;

            int rowId = 0;
            int end = EndPos > count ? count : EndPos;
            for (int k = StartPos; k < end; ++k)
            {
                Row<T> row = rows[rowId];
                T datum = data[k];
                row.SetReference(datum);
                row.SetInteractable(true);
                rowId++;
            }

            // Disable unused
            for (int k = rowId; k < rows.Count; ++k)
            {
                Row<T> row = rows[k];
                row.RemoveReference();
                row.SetInteractable(false);
            }

            UpdateRowHighlight();
        }

        private void OnVerticalScrollbar(float value)
        {
            int count = data.Count;
            int extracount = count - rows.Count;
            extracount = extracount < 0 ? 0 : extracount;
            float size = (extracount + rows.Count == 0) ?
                         1 :
                         (float)rows.Count / (extracount + rows.Count);
            verticalScrollbar.size = size;

            float pos = verticalScrollbar.value;
            StartPos = Mathf.RoundToInt(pos * extracount);
            ClampStartPos();
            BuildList();
        }

        private void UpdateVerticalScrollbar()
        {
            int count = data.Count;
            int extracount = count - rows.Count;
            extracount = extracount < 0 ? 0 : extracount;
            float size = (extracount + rows.Count == 0) ?
                         1 :
                         (float)rows.Count / (extracount + rows.Count);
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

        private void UpdateRowHighlight()
        {
            foreach (var row in rows)
            {
                row.Highlighted = (Selected != null) && ReferenceEquals(row.Reference, Selected);
            }
        }

        private void UnfocusRowFields()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void ClampStartPos()
        {
            StartPos = Mathf.Clamp(StartPos, 0, Mathf.Max(0, data.Count - rows.Count));
        }
    }
}