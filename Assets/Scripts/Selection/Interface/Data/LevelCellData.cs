using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;

namespace ArcCreate.Selection.Interface
{
    public class LevelCellData : CellData
    {
        public LevelStorage LevelStorage { get; set; }

        public LevelList LevelList { get; internal set; }
    }
}