using ArcCreate.Compose.Popups;

namespace ArcCreate.Compose.Project
{
    public struct ChartFault
    {
        public Severity Severity;
        public int LineNumber;
        public int StartCharPos;
        public int EndCharPos;
        public string Description;
    }
}