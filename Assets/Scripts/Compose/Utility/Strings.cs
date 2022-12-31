namespace ArcCreate.Compose
{
    public static class Strings
    {
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
    }
}