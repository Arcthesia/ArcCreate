namespace ArcCreate.Data
{
    public enum ClearResult
    {
        Unknown = -1,
        Fail = 0,
        Clear = 10,
        FullCombo = 20,
        AllGood = 21,
        AllPerfect = 22,
        Max = 23,
    }

    public enum Grade
    {
        EXPlus = 9_900_000,
        EX = 9_800_000,
        AA = 9_500_000,
        A = 9_200_000,
        B = 8_900_000,
        C = 8_600_000,
        D = 0,
        Unknown = -1,
    }
}