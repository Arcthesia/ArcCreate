using System;

namespace ArcCreate.Storage
{
    public class ImportInformation
    {
        public const string FileName = "import.yml";
        public const string LevelType = "level";
        public const string PackType = "pack";

        public string Directory { get; set; }

        public string Identifier { get; set; }

        public string SettingsFile { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Type { get; set; }
    }
}