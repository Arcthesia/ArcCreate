namespace ArcCreate.Data
{
    public class ChartSettings
    {
        public string ChartPath { get; set; }

        public string AudioPath { get; set; }

        public string JacketPath { get; set; }

        public float BaseBpm { get; set; }

        public bool SyncBaseBpm { get; set; } = true;

        public string BackgroundPath { get; set; }

        public string VideoPath { get; set; }

        public string Title { get; set; }

        public string Composer { get; set; }

        public string Charter { get; set; }

        public string Illustrator { get; set; }

        public string Difficulty { get; set; }

        public float ChartConstant { get; set; }

        public string DifficultyColor { get; set; }

        public SkinSettings Skin { get; set; }

        public ColorSettings Colors { get; set; }

        public PoolSettings PoolSize { get; set; }

        public int LastWorkingTiming { get; set; }

        public ChartSettings Clone()
        {
            return new ChartSettings()
            {
                Title = Title,
                Composer = Composer,
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