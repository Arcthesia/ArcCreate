using System.Threading;
using ArcCreate.Storage;
using ArcCreate.Utility.InfiniteScroll;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcCreate.Selection.Interface
{
    public class GroupCell : Cell, IPointerClickHandler
    {
        [SerializeField] private StorageData storage;
        [SerializeField] private TMP_Text text;
        [SerializeField] private RectTransform icon;
        [SerializeField] private GameObject expandedIcon;
        [SerializeField] private GameObject collapsedIcon;
        [SerializeField] private float offsetLeft;

        public override UniTask LoadCellFully(CellData cellData, CancellationToken cancellationToken)
        {
            return default;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Services.Select.IsAnySelected || storage.IsTransitioning)
            {
                return;
            }

            ToggleCollapse();
            expandedIcon.SetActive(!HierarchyData.IsCollapsed);
            collapsedIcon.SetActive(HierarchyData.IsCollapsed);
        }

        public override void SetCellData(CellData cellData)
        {
            GroupCellData groupCellData = cellData as GroupCellData;
            text.text = groupCellData.Title;
            float textWidth = text.preferredWidth;
            icon.anchoredPosition = new Vector2(
                textWidth + offsetLeft,
                icon.anchoredPosition.y);
            expandedIcon.SetActive(!HierarchyData.IsCollapsed);
            collapsedIcon.SetActive(HierarchyData.IsCollapsed);
        }
    }
}