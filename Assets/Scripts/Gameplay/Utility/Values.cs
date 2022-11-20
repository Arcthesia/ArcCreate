using UnityEngine;

namespace ArcCreate.Gameplay
{
    public static class Values
    {
        public const float TrackLength = 100;
        public const float NoteFadeOutLength = 10;
        public const float LaneWidth = 4.25f;
        public const float ArcY0 = 1f;
        public const float ArcY1 = 5.5f;
        public const float MinVerticalFeedbackY = (ArcY1 * 0.75f) + (ArcY0 * 0.25f);
        public const float MaxLaneFeedbackY = (ArcY1 * 0.5f) + (ArcY0 * 0.5f);
        public const float LaneFeedbackFadeoutDuration = 0.25f;
        public const float LaneFeedbackMaxAlpha = 0.25f;

        public const int LostJudgeWindow = 150;
        public const int FarJudgeWindow = 100;
        public const int PureJudgeWindow = 50;
        public const int MaxJudgeWindow = 25;

        public const int HoldFlashCycle = 4;
        public const int ArcFlashCycle = 5;
        public const float MaxHoldAlpha = 0.8627451f;
        public const float MaxArcAlpha = 0.8823592f;
        public const float FlashArcAlphaScalar = 0.85f;
        public const float FlashHoldAlphaScalar = 0.85f;
        public const float MissedArcAlphaScalar = 0.65f;
        public const float MissedHoldAlphaScalar = 0.5f;
        public const float HoldLengthScalar = 1 / 3.79f;

        public const float ArcTapMiddleWorldXRange = 0.5f;

        public const float ArcOffsetNormal = 0.15f;
        public const float ArcOffsetVoid = 0.9f;

        public const float TraceAlphaScalar = 0.4779405f;
        public const float ShortTraceAlphaScalar = TraceAlphaScalar * 0.5f;

        public const int MaxScore = 10_000_000;
        public const float ScoreModifyDelay = 0.5f;
        public const float FarPenaltyMultipler = 0.5f;

        public static int Offset { get; set; } = 0;

        public static float BaseBpm { get; set; } = 100;

        public static float TimingPointDensity { get; set; } = 1;

        public static Color[] DefaultDifficultyColors { get; set; } = new Color[] { };

        public static bool IsRendering { get; set; } = false; // no idea where else to put it

        public static float LaneFeedbackFrom { get; set; } = 1;

        public static float LaneFeedbackTo { get; set; } = 4;
    }
}