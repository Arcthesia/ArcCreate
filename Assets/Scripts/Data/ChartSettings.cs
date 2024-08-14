using System.Collections.Generic;

namespace ArcCreate.Data
{
    public class ChartSettings
    {
        public string ChartPath { get; set; }

        public string AudioPath { get; set; }

        public string JacketPath { get; set; }

        public float BaseBpm { get; set; }

        public string BpmText { get; set; }

        public bool SyncBaseBpm { get; set; }

        public string BackgroundPath { get; set; }

        public string VideoPath { get; set; }

        public string Title { get; set; }

        public string Composer { get; set; }

        public string Charter { get; set; }

        public string Alias { get; set; }

        public string Illustrator { get; set; }

        public string Difficulty { get; set; }

        public double ChartConstant { get; set; }

        public string DifficultyColor { get; set; }

        public SkinSettings Skin { get; set; }

        public ColorSettings Colors { get; set; }

        public int LastWorkingTiming { get; set; }

        public int PreviewStart { get; set; }

        public int PreviewEnd { get; set; }

        public string SearchTags { get; set; }

        public ChartSettings Clone()
        {
            return new ChartSettings()
            {
                Title = Title,
                Composer = Composer,
                Charter = Charter,
                Alias = Alias,
                Illustrator = Illustrator,
                BaseBpm = BaseBpm,
                BpmText = BpmText,
                SyncBaseBpm = SyncBaseBpm,
                Difficulty = Difficulty,
                ChartConstant = ChartConstant,
                DifficultyColor = DifficultyColor,
                Skin = Skin?.Clone(),
                Colors = Colors?.Clone(),
                LastWorkingTiming = LastWorkingTiming,
                ChartPath = ChartPath,
                AudioPath = AudioPath,
                JacketPath = JacketPath,
                BackgroundPath = BackgroundPath,
                VideoPath = VideoPath,
                PreviewStart = PreviewStart,
                PreviewEnd = PreviewEnd,
                SearchTags = SearchTags,
            };
        }

        public IEnumerable<string> GetReferencedFiles()
        {
            yield return ChartPath;
            yield return AudioPath;
            yield return JacketPath;
            yield return BackgroundPath;
            yield return VideoPath;
        }

        public bool IsSameDifficulty(ChartSettings other, bool parseDifficutyName = true)
        {
            if (other == null)
            {
                return false;
            }

            return IsSameDifficulty(other.ChartPath, other.Difficulty, parseDifficutyName);
        }

        public bool IsSameDifficulty(string chartPath, string difficultyName, bool parseDifficutyName = true)
        {
            int thisRightDot = ChartPath.LastIndexOf('.');
            int otherRightDot = chartPath.LastIndexOf('.');

            bool isSame = true;
            if (thisRightDot != otherRightDot)
            {
                isSame = false;
            }
            else
            {
                for (int i = 0; i < thisRightDot; i++)
                {
                    if (ChartPath[i] != chartPath[i])
                    {
                        isSame = false;
                        break;
                    }
                }
            }

            if (isSame)
            {
                return true;
            }

            if (!parseDifficutyName)
            {
                return false;
            }

            // The difficulty name check is only meant for custom chart file names.
            // If either chart file has 1 character as its name then treat it as internal difficulty type.
            // A bit hacky but it's ok
            if (thisRightDot == 1 || otherRightDot == 1)
            {
                return false;
            }

            if (string.IsNullOrEmpty(Difficulty) || string.IsNullOrEmpty(difficultyName))
            {
                return false;
            }

            int thisRightSpace = Difficulty.LastIndexOf(' ');
            int otherRightSpace = difficultyName.LastIndexOf(' ');

            if (thisRightSpace == -1 || otherRightSpace == -1 || thisRightSpace != otherRightSpace)
            {
                return false;
            }

            for (int i = 0; i < thisRightSpace; i++)
            {
                if (Difficulty[i] != difficultyName[i])
                {
                    return false;
                }
            }

            return true;
        }

        public (int diff, bool isPlus) ParseChartConstant()
        {
            int roundDown = (int)ChartConstant;

            bool isPlus = roundDown >= 7 && (ChartConstant - roundDown) >= 0.69999;

            return (roundDown, isPlus);
        }

        public (string name, string number) ParseDifficultyName(int maxNumberLength)
        {
            if (string.IsNullOrEmpty(Difficulty))
            {
                return (string.Empty, string.Empty);
            }

            int lastSpaceIndex = Difficulty.LastIndexOf(' ');
            if (lastSpaceIndex < 0 || lastSpaceIndex >= Difficulty.Length)
            {
                return Difficulty.Length > maxNumberLength ? (Difficulty, string.Empty) : (string.Empty, Difficulty);
            }

            return (Difficulty.Substring(0, lastSpaceIndex), Difficulty.Substring(lastSpaceIndex + 1));
        }
    }
}