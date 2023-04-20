using System;
using UltraLiteDB;

namespace ArcCreate.Storage.Data
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
        public float GaugeValue;
        public float GaugeClearRequirement;
        public float GaugeMax;

        [BsonIgnore] public int PerfectCount => LatePerfectCount + EarlyPerfectCount + MaxCount;

        [BsonIgnore] public int GoodCount => LateGoodCount + EarlyGoodCount;

        [BsonIgnore] public int MissCount => LateMissCount + EarlyMissCount;

        [BsonIgnore] public int NoteCount => PerfectCount + GoodCount + MissCount;

        [BsonIgnore]
        public float Score
        {
            get
            {
                float res = 0;
                float scorePerNote = Gameplay.Values.MaxScore / NoteCount;
                res += GoodCount * scorePerNote * Gameplay.Values.GoodPenaltyMultipler;
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
    }
}