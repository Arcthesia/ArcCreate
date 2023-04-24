using System;
using System.Text;
using UltraLiteDB;

namespace ArcCreate.Data
{
    public class PlayResult
    {
        public DateTime DateTime { get; set; }

        public int LateMissCount { get; set; }

        public int LateGoodCount { get; set; }

        public int LatePerfectCount { get; set; }

        public int MaxCount { get; set; }

        public int EarlyPerfectCount { get; set; }

        public int EarlyGoodCount { get; set; }

        public int EarlyMissCount { get; set; }

        public int MaxCombo { get; set; }

        public int RetryCount { get; set; }

        public float GaugeValue { get; set; }

        public float GaugeClearRequirement { get; set; }

        public float GaugeMax { get; set; }

        public double BestScore { get; set; }

        public int PlayCount { get; set; }

        public int NoteCount { get; set; }

        [BsonIgnore]
        public int PerfectCount => LatePerfectCount + EarlyPerfectCount + MaxCount;

        [BsonIgnore]
        public int GoodCount => LateGoodCount + EarlyGoodCount;

        [BsonIgnore]
        public int MissCount => LateMissCount + EarlyMissCount;

        [BsonIgnore]
        public double Score
        {
            get
            {
                double res = 0;
                if (NoteCount == 0)
                {
                    return 0;
                }

                double scorePerNote = (double)Constants.MaxScore / NoteCount;
                res += GoodCount * scorePerNote * Constants.GoodPenaltyMultipler;
                res += PerfectCount * scorePerNote;
                res += MaxCount;
                return res;
            }
        }

        [BsonIgnore]
        public ClearResult ClearResult
        {
            get
            {
                if (NoteCount == 0)
                {
                    return ClearResult.Unknown;
                }

                if (MaxCount == NoteCount)
                {
                    return ClearResult.Max;
                }

                if (PerfectCount == NoteCount)
                {
                    return ClearResult.AllPerfect;
                }

                if (GoodCount == NoteCount)
                {
                    return ClearResult.AllGood;
                }

                if (MaxCombo == NoteCount)
                {
                    return ClearResult.FullCombo;
                }

                if (GaugeValue > GaugeClearRequirement)
                {
                    return ClearResult.Clear;
                }

                return ClearResult.Fail;
            }
        }

        [BsonIgnore]
        public Grade Grade
        {
            get
            {
                double score = Score;
                Grade result = Grade.D;
                foreach (Grade grade in Enum.GetValues(typeof(Grade)))
                {
                    if (score > (double)grade && (double)grade > (double)result)
                    {
                        result = grade;
                    }
                }

                return result;
            }
        }

        public string FormattedScore => FormatScore(Score);

        public static string FormatScore(double score)
        {
            string s = ((int)Math.Round(score)).ToString("D8");
            StringBuilder sb = new StringBuilder();
            for (int i = s.Length - 1; i >= 0; i--)
            {
                sb.Insert(0, s[i]);
                if ((i + 1) % 3 == 0 && i != 0)
                {
                    sb.Insert(0, '\'');
                }
            }

            return sb.ToString();
        }
    }
}