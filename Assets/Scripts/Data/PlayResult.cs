using System;
using UltraLiteDB;

namespace ArcCreate.Data
{
    public struct PlayResult
    {
        public DateTime DateTime;
        public int LateMissCount;
        public int LateGoodCount;
        public int LatePerfectCount;
        public int MaxCount;
        public int EarlyPerfectCount;
        public int EarlyGoodCount;
        public int EarlyMissCount;
        public int MaxCombo;
        public int RetryCount;
        public float GaugeValue;
        public float GaugeClearRequirement;
        public float GaugeMax;
        public double BestScore;
        public int PlayCount;
        public int NoteCount;

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
                    return ClearResult.AllPerfect;
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
                    if (score > (double)grade)
                    {
                        result = grade;
                    }
                }

                return result;
            }
        }
    }
}