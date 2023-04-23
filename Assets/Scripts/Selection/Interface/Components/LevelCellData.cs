using ArcCreate.Data;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;

namespace ArcCreate.Selection.Interface
{
    public class LevelCellData : CellData
    {
        public LevelStorage LevelStorage { get; set; }

        public PlayHistory PlayHistory { get; set; }

        public ChartSettings ChartToDisplay { get; set; }
    }
}