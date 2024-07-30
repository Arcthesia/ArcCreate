using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Utility.InfiniteScroll
{
    public class InfiniteScroll : MonoBehaviour, IDragHandler
    {
        /// <summary>
        /// User defined cell data.
        /// </summary>
        private readonly List<CellData> data = new List<CellData>();

        /// <summary>
        /// Store hierarchy data necessary for rendering. Should be a 1-1 correspondance with dataSource.
        /// </summary>
        private readonly List<HierarchyData> hierarchy = new List<HierarchyData>();

        private readonly List<Cell> visibleCells = new List<Cell>();

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private float marginTop;
        [SerializeField] private float marginBottom;
        [SerializeField] private float marginLeft;
        [SerializeField] private float marginRight;
        [SerializeField] private float spacing;
        [SerializeField] private RectTransform.Axis axis;
        [SerializeField] private bool useTwoStageLoading;
        [SerializeField] private float maxVelocityForSecondStage;
        private RectTransform contentRect;
        private RectTransform containerRect;
        private bool setup;
        private Vector2 previousContentRectPosition;

        public event Action OnPointerEvent;

        public float Value
        {
            get
            {
                return IsVertical ?
                       contentRect.anchoredPosition.y + (containerRect.rect.height / 2) :
                       -contentRect.anchoredPosition.x + (containerRect.rect.width / 2);
            }

            set
            {
                if (hierarchy.Count == 0)
                {
                    return;
                }

                if (IsVertical)
                {
                    float halfContainerHeight = containerRect.rect.height / 2;
                    float contentHeight = contentRect.rect.height;
                    float max = contentHeight < containerRect.rect.height ?
                        halfContainerHeight :
                        contentHeight - halfContainerHeight;
                    value = Mathf.Clamp(value, halfContainerHeight, max);
                    contentRect.anchoredPosition = new Vector2(
                        contentRect.anchoredPosition.x,
                        value - halfContainerHeight);
                }
                else
                {
                    float halfContainerWidth = containerRect.rect.width / 2;
                    float contentWidth = contentRect.rect.width;
                    float max = contentWidth < containerRect.rect.width ?
                        halfContainerWidth :
                        contentWidth - halfContainerWidth;
                    value = Mathf.Clamp(value, halfContainerWidth, max);
                    contentRect.anchoredPosition = new Vector2(
                        -value + (containerRect.rect.width / 2),
                        contentRect.anchoredPosition.y);
                }
            }
        }

        public List<CellData> Data => data;

        public List<HierarchyData> Hierarchy => hierarchy;

        private bool IsVertical => axis == RectTransform.Axis.Vertical;

        public void SetData(List<CellData> data)
        {
            if (!setup)
            {
                Awake();
            }

            this.data.Clear();
            hierarchy.Clear();

            foreach (Cell cell in visibleCells)
            {
                cell.CellData.Pool.Return(cell);
            }

            visibleCells.Clear();

            foreach (CellData cellData in data)
            {
                AddCell(cellData);
            }

            RecalculateCellsState();
            Rebuild();
            LoadSecondStage(0);
        }

        /// <summary>
        /// Collapse a cell. Any children cell will be hidden from view.
        /// </summary>
        /// <param name="cellIndex">Flat index of cell.</param>
        public void ToggleCollapse(int cellIndex)
        {
            hierarchy[cellIndex].IsCollapsed = !hierarchy[cellIndex].IsCollapsed;
            RecalculateCellsState();
            Rebuild(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnPointerEvent?.Invoke();
        }

        public void OnDrag(BaseEventData eventData)
        {
            OnPointerEvent?.Invoke();
        }

        /// <summary>
        /// Add cell to cells list along with their children.
        /// </summary>
        private void AddCell(CellData cellData, int parent = -1, int indent = 0)
        {
            int index = data.Count;

            data.Add(cellData);
            hierarchy.Add(new HierarchyData(index, cellData.Size, indent, parent)
            {
                IsCollapsed = cellData.CollapsedByDefault,
                SecondStageStarted = false,
            });

            if (cellData.Children != null)
            {
                foreach (CellData child in cellData.Children)
                {
                    AddCell(child, index, indent + 1);
                }
            }
        }

        private void Awake()
        {
            scrollRect.onValueChanged.AddListener(OnScroll);
            contentRect = scrollRect.content;
            containerRect = scrollRect.GetComponent<RectTransform>();

            if (IsVertical)
            {
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
            }
            else
            {
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(0, 1);
            }

            contentRect.pivot = new Vector2(0, 1);
            containerRect.anchoredPosition = Vector2.zero;
            setup = true;
        }

        private void OnDestroy()
        {
            scrollRect.onValueChanged.RemoveListener(OnScroll);
        }

        private void OnScroll(Vector2 val)
        {
            Rebuild();
        }

        private void Update()
        {
            if (useTwoStageLoading)
            {
                Vector2 velocity = contentRect.anchoredPosition - previousContentRectPosition;
                velocity /= Time.deltaTime;
                LoadSecondStage(IsVertical ? velocity.y : velocity.x);

                previousContentRectPosition = contentRect.anchoredPosition;
            }
        }

        private void RecalculateCellsState()
        {
            float positionSoFar = IsVertical ? marginTop : marginLeft;
            for (int i = 0; i < hierarchy.Count; i++)
            {
                HierarchyData item = hierarchy[i];
                item.IsVisible = IsVisible(i);
                item.PositionInRect = positionSoFar;
                if (item.IsVisible)
                {
                    positionSoFar += item.Size + spacing;
                }
            }

            positionSoFar += IsVertical ? marginBottom : marginRight;
            contentRect.SetSizeWithCurrentAnchors(axis, (float)positionSoFar);

            for (int i = 0; i < visibleCells.Count; i++)
            {
                Cell cell = visibleCells[i];
                HierarchyData item = cell.HierarchyData;
                ApplyCellRect(cell, item);
            }
        }

        private bool IsVisible(int index)
        {
            HierarchyData cell = hierarchy[index];
            if (cell.ParentIndex == -1)
            {
                return true;
            }

            HierarchyData parent = hierarchy[cell.ParentIndex];
            if (parent.IsCollapsed)
            {
                return false;
            }

            return IsVisible(cell.ParentIndex);
        }

        public void Rebuild(bool checkInbetween = false)
        {
            float minVisiblePositionInRect = IsVertical
                                           ? contentRect.anchoredPosition.y
                                           : -contentRect.anchoredPosition.x;
            float maxVisiblePositionInRect = IsVertical
                                           ? contentRect.anchoredPosition.y + containerRect.rect.height
                                           : -contentRect.anchoredPosition.x + containerRect.rect.width;

            if (visibleCells.Count == 0)
            {
                for (int i = 0; i <= hierarchy.Count - 1; i++)
                {
                    HierarchyData item = hierarchy[i];
                    bool cellVisible = item.PositionInRect + item.Size >= minVisiblePositionInRect
                                    && item.PositionInRect <= maxVisiblePositionInRect;
                    if (!cellVisible || !item.IsVisible)
                    {
                        continue;
                    }

                    CellData cellData = data[i];
                    Cell cell = cellData.Pool.Get(contentRect);
                    ApplyCell(cell, cellData, item);
                    visibleCells.Add(cell);
                }

                return;
            }

            int minVisibleIndex = hierarchy.Count;
            int maxVisibleIndex = -1;

            for (int i = visibleCells.Count - 1; i >= 0; i--)
            {
                Cell cell = visibleCells[i];
                HierarchyData item = cell.HierarchyData;
                bool cellVisible = item.PositionInRect + item.Size >= minVisiblePositionInRect
                                && item.PositionInRect <= maxVisiblePositionInRect;

                if (cellVisible)
                {
                    minVisibleIndex = Mathf.Min(minVisibleIndex, item.IndexFlat);
                    maxVisibleIndex = Mathf.Max(maxVisibleIndex, item.IndexFlat);
                }

                if (!cellVisible || !item.IsVisible)
                {
                    visibleCells.RemoveAt(i);
                    cell.CellData.Pool.Return(cell);
                    cell.CancelLoadCellFully();
                    item.SecondStageStarted = false;
                    item.IsFullyLoaded = false;
                }
            }

            if (checkInbetween)
            {
                for (int i = minVisibleIndex; i < maxVisibleIndex; i++)
                {
                    HierarchyData item = hierarchy[i];
                    bool cellVisible = item.PositionInRect + item.Size >= minVisiblePositionInRect
                                    && item.PositionInRect <= maxVisiblePositionInRect;

                    if (!item.IsVisible || !cellVisible)
                    {
                        continue;
                    }

                    bool isAlreadyVisible = false;
                    foreach (var visible in visibleCells)
                    {
                        if (visible.HierarchyData.IndexFlat == i)
                        {
                            isAlreadyVisible = true;
                            break;
                        }
                    }

                    if (!isAlreadyVisible)
                    {
                        CellData cellData = data[i];
                        Cell cell = cellData.Pool.Get(contentRect);
                        ApplyCell(cell, cellData, item);
                        visibleCells.Add(cell);
                    }
                }
            }

            for (int i = minVisibleIndex - 1; i >= 0; i--)
            {
                HierarchyData item = hierarchy[i];
                bool cellVisible = item.PositionInRect + item.Size >= minVisiblePositionInRect
                                && item.PositionInRect <= maxVisiblePositionInRect;
                if (!cellVisible)
                {
                    break;
                }

                if (!item.IsVisible)
                {
                    continue;
                }

                CellData cellData = data[i];
                Cell cell = cellData.Pool.Get(contentRect);
                ApplyCell(cell, cellData, item);
                visibleCells.Add(cell);
            }

            for (int i = maxVisibleIndex + 1; i < hierarchy.Count; i++)
            {
                HierarchyData item = hierarchy[i];
                bool cellVisible = item.PositionInRect + item.Size >= minVisiblePositionInRect
                                && item.PositionInRect <= maxVisiblePositionInRect;
                if (!cellVisible)
                {
                    break;
                }

                if (!item.IsVisible)
                {
                    continue;
                }

                CellData cellData = data[i];
                Cell cell = cellData.Pool.Get(contentRect);
                ApplyCell(cell, cellData, item);
                visibleCells.Add(cell);
            }
        }

        private void ApplyCell(Cell cell, CellData cellData, HierarchyData hierarchyData)
        {
            ApplyCellRect(cell, hierarchyData);
            cell.Scroll = this;
            cell.HierarchyData = hierarchyData;
            cell.CellData = cellData;
            cell.SetCellData(cellData);
        }

        private void ApplyCellRect(Cell cell, HierarchyData hierarchyData)
        {
            if (IsVertical)
            {
                cell.RectTransform.anchorMin = new Vector2(0, 1);
                cell.RectTransform.anchorMax = new Vector2(1, 1);
                cell.RectTransform.offsetMin = new Vector2(marginLeft, 0);
                cell.RectTransform.offsetMax = new Vector2(marginRight, 0);
                cell.RectTransform.anchoredPosition = new Vector2(0, -hierarchyData.PositionInRect);
            }
            else
            {
                cell.RectTransform.anchorMin = new Vector2(0, 0);
                cell.RectTransform.anchorMax = new Vector2(0, 1);
                cell.RectTransform.offsetMin = new Vector2(0, marginBottom);
                cell.RectTransform.offsetMax = new Vector2(0, marginTop);
                cell.RectTransform.anchoredPosition = new Vector2(hierarchyData.PositionInRect, 0);
            }

            cell.RectTransform.SetSizeWithCurrentAnchors(axis, hierarchyData.Size);
        }

        private void LoadSecondStage(float velocity)
        {
            if (Mathf.Abs(velocity) > maxVelocityForSecondStage)
            {
                return;
            }

            float minVisiblePositionInRect = IsVertical
                                           ? contentRect.anchoredPosition.y
                                           : -contentRect.anchoredPosition.x;
            float maxVisiblePositionInRect = IsVertical
                                           ? contentRect.anchoredPosition.y + containerRect.rect.height
                                           : -contentRect.anchoredPosition.x + containerRect.rect.width;

            foreach (var cell in visibleCells)
            {
                float predictedMinVisibleAfterLoad = minVisiblePositionInRect + (velocity * cell.PredictedLoadTime);
                float predictedMaxVisibleAfterLoad = maxVisiblePositionInRect + (velocity * cell.PredictedLoadTime);

                HierarchyData item = cell.HierarchyData;
                bool willBeVisible = item.PositionInRect + item.Size >= predictedMinVisibleAfterLoad
                                  && item.PositionInRect <= predictedMaxVisibleAfterLoad;

                if (willBeVisible && !item.SecondStageStarted)
                {
                    cell.SetCellDataFully(cell.CellData).Forget();
                    item.SecondStageStarted = true;
                }

                if (!willBeVisible && item.SecondStageStarted && !item.IsFullyLoaded)
                {
                    cell.CancelLoadCellFully();
                    item.SecondStageStarted = false;
                }
            }
        }
    }
}