namespace ArcCreate.Gameplay.Judgement
{
    public static class JudgementResultExtension
    {
        public static JudgementResult CalculateJudgeResult(this int inputTime, int trueTime)
        {
            return CalculateJudgeResult(inputTime - trueTime);
        }

        public static JudgementResult CalculateJudgeResult(this int offset)
        {
            if (offset >= Values.GoodJudgeWindow)
            {
                return JudgementResult.MissLate;
            }

            if (offset >= Values.PerfectJudgeWindow)
            {
                return JudgementResult.GoodLate;
            }

            if (offset >= Values.MaxJudgeWindow)
            {
                return JudgementResult.PerfectLate;
            }

            if (offset >= -Values.MaxJudgeWindow)
            {
                return JudgementResult.Max;
            }

            if (offset >= -Values.PerfectJudgeWindow)
            {
                return JudgementResult.PerfectEarly;
            }

            if (offset >= -Values.GoodJudgeWindow)
            {
                return JudgementResult.GoodEarly;
            }

            return JudgementResult.MissEarly;
        }

        public static bool IsEarly(this JudgementResult res)
        {
            return res == JudgementResult.GoodEarly
                || res == JudgementResult.PerfectEarly;
        }

        public static bool IsLate(this JudgementResult res)
        {
            return res == JudgementResult.GoodLate
                || res == JudgementResult.PerfectLate;
        }

        public static bool IsMiss(this JudgementResult res)
        {
            return res == JudgementResult.MissEarly
                || res == JudgementResult.MissLate
                || res == JudgementResult.MissMapped;
        }

        public static bool IsGood(this JudgementResult res)
        {
            return res == JudgementResult.GoodEarly
                || res == JudgementResult.GoodLate
                || res == JudgementResult.GoodMapped;
        }

        public static bool IsPerfect(this JudgementResult res)
        {
            return res == JudgementResult.PerfectEarly
                || res == JudgementResult.Max
                || res == JudgementResult.PerfectLate
                || res == JudgementResult.PerfectMapped;
        }

        public static bool IsMax(this JudgementResult res)
        {
            return res == JudgementResult.Max;
        }
    }
}