using ArcCreate.Compose.Popups;

namespace ArcCreate.Compose.Project
{
    public struct ChartFault
    {
        public Severity Severity;
        public Option<int> LineNumber;
        public Option<int> StartCharPos;
        public Option<int> Length;
        public string Description;
    }
}