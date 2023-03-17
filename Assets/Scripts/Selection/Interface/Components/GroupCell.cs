using System.Threading;
using ArcCreate.Utility.InfiniteScroll;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ArcCreate.Selection.Interface
{
    public class GroupCell : Cell
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private RectTransform seperatorRight;
        [SerializeField] private float offsetLeft;

        public override UniTask LoadCellFully(CellData cellData, CancellationToken cancellationToken)
        {
            return default;
        }

        public override void SetCellData(CellData cellData)
        {
            GroupCellData groupCellData = cellData as GroupCellData;
            text.text = groupCellData.Title;
            float textWidth = text.preferredWidth;
            seperatorRight.offsetMin = new Vector2(
                textWidth + offsetLeft,
                seperatorRight.offsetMin.y);
        }
    }
}