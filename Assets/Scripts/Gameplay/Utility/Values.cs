using UnityEngine;

namespace ArcCreate.Gameplay
{
    public static class Values
    {
        // Playfield
        public const float TrackLengthForward = 100;
        public const float TrackLengthBackward = 53.5f;
        public const float MinInputLaneZ = TrackLengthForward / 10f;
        public const float LaneWidth = 4.25f;
        public const float NoteFadeOutLength = 10;
        public const float ArcY0 = 1f;
        public const float ArcY1 = 5.5f;

        // Input feedback
        public const float MinVerticalFeedbackY = (ArcY1 * 0.25f) + (ArcY0 * 0.75f);
        public const float MaxLaneFeedbackY = (ArcY1 * 0.5f) + (ArcY0 * 0.5f);
        public const float LaneFeedbackFadeoutDuration = 0.15f;
        public const float LaneFeedbackMaxAlpha = 0.15f;

        // Judgement
        public const int LostJudgeWindow = 150;
        public const int FarJudgeWindow = 100;
        public const int PureJudgeWindow = 50;
        public const int MaxJudgeWindow = 25;

        // Visual
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
        public const float TextParticleYOffset = 0.75f;

        // Judgement
        public const int MaxScore = 10_000_000;
        public const int ScoreModifyDelay = 500;
        public const float FarPenaltyMultipler = 0.5f;
        public const int ArcLockDuration = 500;
        public const int ArcGraceDuration = 1000;
        public const int ArcRedFlashCycle = 500;
        public const float ComboLostFlashDuration = 0.1f;

        // I sure hope no charter will make use of lane -2147483648
        public const int InvalidLane = int.MinValue;

        public static int ChartAudioOffset { get; set; } = 0;

        public static float BaseBpm { get; set; } = 100;

        public static float TimingPointDensity { get; set; } = 1;

        public static Color[] DefaultDifficultyColors { get; set; } = new Color[] { };

        public static bool IsRendering { get; set; } = false; // no idea where else to put it

        public static float LaneFrom { get; set; } = 1;

        public static float LaneTo { get; set; } = 4;

        public static float TapScreenHitbox { get; set; } = 1;
    }
}