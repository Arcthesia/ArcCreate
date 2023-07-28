using System.Collections.Generic;

namespace ArcCreate.Data
{
    public class EditorProjectSettings
    {
        public string LastUsedPublisher { get; set; }

        public string LastUsedPackageName { get; set; }

        public int LastUsedVersionNumber { get; set; } = 0;

        public Dictionary<string, List<Timestamp>> Timestamps { get; set; }
    }
}