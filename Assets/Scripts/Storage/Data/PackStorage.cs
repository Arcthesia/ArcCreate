using System.Collections.Generic;
using ArcCreate.Data;
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

        public struct PackImportInformation
        {
            public string PackName { get; set; }

            public string ImagePath { get; set; }

            public List<string> LevelIdentifiers { get; set; }
        }
    }
}