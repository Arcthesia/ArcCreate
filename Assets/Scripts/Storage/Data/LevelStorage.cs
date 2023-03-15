using System;
using ArcCreate.Data;

namespace ArcCreate.Storage.Data
{
    public class LevelStorage : StorageUnit<LevelStorage>
    {
        public override string Type => "Level";

        public ProjectSettings Settings { get; set; }
    }
}