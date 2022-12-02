namespace ArcCreate.Gameplay.Judgement
{
    public static class JudgementResultExtension
    {
        public static JudgementResult CalculateJudgeResult(this int inputTime, int trueTime)
        {
            if (inputTime >= trueTime + Values.FarJudgeWindow)
            {
                return JudgementResult.LostLate;
            }

            if (inputTime >= trueTime + Values.PureJudgeWindow)
            {
                return JudgementResult.FarLate;
            }

            if (inputTime >= trueTime + Values.MaxJudgeWindow)
            {
                return JudgementResult.PureLate;
            }

            if (inputTime >= trueTime - Values.MaxJudgeWindow)
            {
                return JudgementResult.Max;
            }

            if (inputTime >= trueTime - Values.PureJudgeWindow)
            {
                return JudgementResult.PureEarly;
            }

            if (inputTime >= trueTime - Values.FarJudgeWindow)
            {
                return JudgementResult.FarEarly;
            }

            /*--------------------------------------------------*/
            return JudgementResult.LostEarly;
        }

        public static bool IsEarly(this JudgementResult res)
        {
            return res == JudgementResult.FarEarly
                || res == JudgementResult.PureEarly;
        }

        public static bool IsLate(this JudgementResult res)
        {
            return res == JudgementResult.FarLate
                || res == JudgementResult.PureLate;
        }

        public static bool IsLost(this JudgementResult res)
        {
            return res == JudgementResult.LostEarly
                || res == JudgementResult.LostLate;
        }

        public static bool IsFar(this JudgementResult res)
        {
            return res == JudgementResult.FarEarly
                || res == JudgementResult.FarLate;
        }

        public static bool IsPure(this JudgementResult res)
        {
            return res == JudgementResult.PureEarly
                || res == JudgementResult.Max
                || res == JudgementResult.PureLate;
        }

        public static bool IsMax(this JudgementResult res)
        {
            return res == JudgementResult.Max;
        }
    }
}