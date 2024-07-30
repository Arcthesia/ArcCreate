namespace ArcCreate.Storage
{
    public class ImportInformation
    {
        public const string FileName = "index.yml";
        public const string LevelType = "level";
        public const string PackType = "pack";
        public const string CharacterType = "character";

        public string Directory { get; set; }

        public string Identifier { get; set; }

        public string SettingsFile { get; set; }

        public int Version { get; set; }

        public string Type { get; set; }
    }
}