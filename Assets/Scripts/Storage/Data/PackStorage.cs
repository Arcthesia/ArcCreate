using System.Collections.Generic;
using UltraLiteDB;

namespace ArcCreate.Storage.Data
{
    public class PackStorage : StorageUnit<PackStorage>
    {
        public override string Type => "Pack";

        public string PackName { get; set; }

        public string ImagePath { get; set; }

        public List<string> LevelIdentifiers { get; set; }

        [BsonIgnore] public List<LevelStorage> Levels { get; set; }

        public override bool ValidateSelf(out string reason)
        {
            if (!base.ValidateSelf(out reason))
            {
                return false;
            }

            if (string.IsNullOrEmpty(PackName))
            {
                reason = "Pack name is empty";
                return false;
            }

            if (string.IsNullOrEmpty(ImagePath))
            {
                reason = "Image path is not defined";
                return false;
            }

            if (LevelIdentifiers == null || LevelIdentifiers.Count <= 0)
            {
                reason = "Pack contains no levels";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public struct PackImportInformation
        {
            public string PackName { get; set; }

            public string ImagePath { get; set; }

            public List<string> LevelIdentifiers { get; set; }
        }
    }
}