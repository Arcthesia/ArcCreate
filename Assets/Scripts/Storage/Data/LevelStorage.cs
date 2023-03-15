using System;
using ArcCreate.Data;

namespace ArcCreate.Storage.Data
{
    public class LevelStorage : StorageUnit<LevelStorage>
    {
        public override string Type => "Level";

        public ProjectSettings Settings { get; set; }

        public override bool ValidateSelf(out string reason)
        {
            if (!base.ValidateSelf(out reason))
            {
                return false;
            }

            if (Settings == null)
            {
                reason = "Level settings was not defined";
                return false;
            }

            if (Settings.Charts == null || Settings.Charts.Count <= 0)
            {
                reason = "Level contains no charts";
                return false;
            }

            return true;
        }
    }
}