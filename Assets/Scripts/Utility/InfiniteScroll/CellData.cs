using System.Collections.Generic;

namespace ArcCreate.Utility.InfiniteScroll
{
    public class CellData
    {
        public Pool<Cell> Pool { get; set; }

        public float Size { get; set; }

        public List<CellData> Children { get; set; }
    }
}