using System.Collections.Generic;

namespace ArcCreate.Selection.Interface
{
    public interface ISortStrategy
    {
        List<LevelCellData> Sort(List<LevelCellData> cells);
    }

    public interface ISortPackStrategy
    {
        List<PackCellData> Sort(List<PackCellData> cells);
    }
}