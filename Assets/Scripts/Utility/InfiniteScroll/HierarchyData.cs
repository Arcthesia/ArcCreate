namespace ArcCreate.Utility.InfiniteScroll
{
    public class HierarchyData
    {
        public HierarchyData(int indexFlat, float size, int indentDepth, int parentIndex)
        {
            IndexFlat = indexFlat;
            Size = size;
            IndentDepth = indentDepth;
            ParentIndex = parentIndex;
        }

        public int IndexFlat { get; private set; }

        public float Size { get; private set; }

        public int IndentDepth { get; private set; }

        public int ParentIndex { get; private set; }

        public float PositionInRect { get; set; }

        public bool IsCollapsed { get; set; }

        public bool IsVisible { get; set; }

        public bool SecondStageStarted { get; set; }

        public bool IsFullyLoaded { get; set; }

        public float ValueToCenterCell => PositionInRect + (Size / 2);
    }
}