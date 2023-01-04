namespace ArcCreate.Compose
{
    public static class Values
    {
        public const float DropRateScalar = 60;

        // Extensions
        public const string ProjectExtension = ".arcproj";
        public const string ProjectExtensionWithoutDot = "arcproj";

        public static readonly string[] ImageExtensions
            = new string[] { ".jpg", ".png" };

        public static readonly string[] AudioExtensions
            = new string[] { ".ogg", ".wav", ".mp3" };

        // Names
        public const string DefaultChartFileName = "2";
        public const string DefaultBpm = "100";
        public const string BaseFileName = "base";
        public const string BackgroundFileName = "bg";
        public const string BackgroundFilePrefix = "bg_";
        public const string DefaultTitle = "Untitled";
        public const string DefaultComposer = "N/A";

        // Pools
        public const string TickPoolName = "TickPool";

        public static State<int> EditingTimingGroup { get; } = new State<int>();

        public static State<bool> FullScreen { get; } = new State<bool>();
    }
}