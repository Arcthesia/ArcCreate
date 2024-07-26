using System.Collections.Generic;
using System.Linq;

namespace ArcCreate.Selection.Interface
{
    public class SortPackByAddedDate : ISortPackStrategy
    {
        public const string Typename = "addeddate";

        public List<PackCellData> Sort(List<PackCellData> cells)
        {
            if (cells.Count == 0)
            {
                return cells;
            }

            return cells
                .OrderBy(cell => cell.PackStorage.AddedDate)
                .ToList();
        }
    }
}