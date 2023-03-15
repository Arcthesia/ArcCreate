namespace ArcCreate.Storage.Data
{
    public struct Score
    {
        public int ScoreValue;

        public int MaxCount;

        public int EarlyPureCount;

        public int LatePureCount;

        public int EarlyFarCount;

        public int LateFarCount;

        public int EarlyLostCount;

        public int LateLostCount;

        public ScoreCategory ScoreCategory;
    }
}