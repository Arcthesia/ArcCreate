using System.Collections.Generic;
using ArcCreate.Utility.InfiniteScroll;

namespace ArcCreate.Selection.Interface
{
    public interface IGroupStrategy
    {
        List<CellData> GroupCells(List<LevelCellData> cells, ISortStrategy sortStrategy);
    }
}