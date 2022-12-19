using UnityEngine;

namespace ArcCreate.Compose.Project
{
    public class ChartSettings
    {
        public string Title { get; set; }

        public string Artist { get; set; }

        public string Charter { get; set; }

        public string Illustrator { get; set; }

        public float BaseBpm { get; set; }

        public bool SyncBaseBpm { get; set; } = true;

        public string Difficulty { get; set; }

        public float ChartConstant { get; set; }

        public Color DifficultyColor { get; set; }

        public SkinSettings Skin { get; set; }

        public ColorSettings Colors { get; set; }

        public int LastWorkingTiming { get; set; }

        public string ChartPath { get; set; }

        public string AudioPath { get; set; }

        public string JacketPath { get; set; }

        public string BackgroundPath { get; set; }

        public string VideoPath { get; set; }

        public ChartSettings Clone()
        {
            return new ChartSettings()
            {
                Title = Title,
                Artist = Artist,
                Charter = Charter,
                Illustrator = Illustrator,
                BaseBpm = BaseBpm,
                SyncBaseBpm = SyncBaseBpm,
                Difficulty = Difficulty,
                ChartConstant = ChartConstant,
                DifficultyColor = DifficultyColor,
                Skin = Skin,
                Colors = Colors,
                LastWorkingTiming = LastWorkingTiming,
                AudioPath = AudioPath,
                JacketPath = JacketPath,
                BackgroundPath = BackgroundPath,
                VideoPath = VideoPath,
            };
        }
    }
}