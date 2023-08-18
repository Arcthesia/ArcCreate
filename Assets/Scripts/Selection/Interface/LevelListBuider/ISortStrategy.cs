using System.Collections.Generic;

namespace ArcCreate.Selection.Interface
{
    public interface ISortStrategy
    {
        List<LevelCellData> Sort(List<LevelCellData> cells);
    }
}