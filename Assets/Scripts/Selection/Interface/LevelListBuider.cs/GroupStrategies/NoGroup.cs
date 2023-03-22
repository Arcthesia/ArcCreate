using System.Collections.Generic;
using System.Linq;
using ArcCreate.Utility.InfiniteScroll;

namespace ArcCreate.Selection.Interface
{
    public class NoGroup : IGroupStrategy
    {
        public const string Typename = "None";

        public List<CellData> GroupCells(List<LevelCellData> cells, ISortStrategy sortStrategy)
        {
            return sortStrategy.Sort(cells).ToList<CellData>();
        }
    }
}