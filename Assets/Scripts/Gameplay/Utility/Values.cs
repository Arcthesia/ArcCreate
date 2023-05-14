using UnityEngine;

namespace ArcCreate.Gameplay
{
    public static class Values
    {
        // Playfield
        public const float TrackLengthForward = 100;
        public const float TrackLengthBackward = 53.5f;
        public const float MinInputLaneZ = TrackLengthForward / 20f;
        public const float LaneWidth = 4.25f;
        public const float NoteFadeOutLength = 10;
        public const float ArcY0 = 1f;
        public const float ArcY1 = 5.5f;

        // Input feedback
        public const float MinVerticalFeedbackY = (ArcY1 * 0.25f) + (ArcY0 * 0.75f);
        public const float MaxLaneFeedbackY = (ArcY1 * 0.5f) + (ArcY0 * 0.5f);
        public const float LaneFeedbackFadeoutDuration = 0.15f;
        public const float LaneFeedbackMaxAlpha = 0.15f;

        // Judgement winodw
        public const int MissJudgeWindow = 120;
        public const int GoodJudgeWindow = 100;
        public const int PerfectJudgeWindow = 50;
        public const int MaxJudgeWindow = 25;
        public const int HoldMissLateJudgeWindow = 240;

        // Visual
        public const int HoldFlashCycle = 4;
        public const int ArcFlashCycle = 5;
        public const float MaxHoldAlpha = 0.8627451f;
        public const float MaxArcAlpha = 0.8823592f;
        public const float FlashArcAlphaScalar = 0.85f;
        public const float FlashHoldAlphaScalar = 0.85f;
        public const float MissedArcAlphaScalar = 0.75f;
        public const float MissedHoldAlphaScalar = 0.5f;
        public const int FadingHoldsFadeDelay = 500;
        public const int FadingHoldsFadeDuration = 500;
        public const float HoldLengthScalar = 1 / 3.79f;
        public const float ArcTapMiddleWorldXRange = 0.5f;
        public const float TraceMeshOffset = 0.15f;
        public const float ArcMeshOffset = 0.9f;
        public const float TraceAlphaScalar = 0.4779405f;
        public const float ShortTraceAlphaScalar = TraceAlphaScalar * 0.5f;
        public const float TextParticleYOffset = 60f;
        public const float ArcSegmentLength = 1000f / 14f;
        public const float ArcCapSize = 0.35f;
        public const float ArcCapSizeAdditionMax = 0.5f;
        public const float TraceCapSize = 0.21f;
        public const float ArcCapAlpha = 1f;
        public const float TraceCapAlpha = 0.5f;
        public const float TraceAlpha = 0.4779405f;
        public const int HoldHighlightPersistDuration = 50;
        public const int HoldParticlePersistDuration = 100;
        public const int BeatlineThickness = 20;

        // Judgement
        public const int ScoreModifyDelay = 500;
        public const int ArcLockDuration = 500;
        public const int ArcGraceDuration = 600;
        public const int ArcRedFlashCycle = 500;
        public const float ComboLostFlashDuration = 0.1f;
        public const float ArcHitboxX = 1.9f;
        public const float ArcHitboxY = 2.5f;
        public const float ArcTapHitboxX = 3.02f;
        public const float ArcTapHitboxYDown = 3.1f;
        public const float ArcTapHitboxYUp = 2.5f;
        public const float MinLongNoteTimeIncrement = 0.1f;

        // Camera
        public const float CameraY = 9f;
        public const float CameraZ = 9f;
        public const float CameraZTablet = 8f;
        public const float CameraRotX = 26.565f;
        public const float CameraRotXTablet = 27.378f;
        public const float SkyInputLabelX = -7.1f;
        public const float SkyInputLabelXTablet = -6.5f;
        public const float CameraTiltSpeed = 6f;
        public const float CameraArcPosScalar = 0.05f;

        // Strings
        public const string EarlyText = "EARLY";
        public const string LateText = "LATE";
        public const string BeatlinePoolName = "beatline";
        public const string TapParticlePoolName = "tapparticle";
        public const string ArcParticlePoolName = "arcparticle";
        public const string HoldParticlePoolName = "holdparticle";

        // I sure hope no charter will make use of lane -2147483648
        public const int InvalidLane = int.MinValue;

        public const int DelayBeforeAudioStart = 2000;

        public const int DelayBeforeAudioResume = 200;

        public static int ChartAudioOffset { get; internal set; } = 0;

        public static float BaseBpm { get; set; } = 100;

        public static float TimingPointDensity { get; set; } = 1;

        public static Color[] DefaultDifficultyColors { get; set; } = new Color[] { };

        public static float LaneFrom { get; set; } = 1;

        public static float LaneTo { get; set; } = 4;

        public static bool EnableColliderGeneration { get; set; } = false;

        public static bool EnableArcRebuildSegment { get; set; } = true;

        public static Vector2 LaneScreenHitboxBase { get; set; } = Vector2.one;

        public static Vector2 ScreenSizeBase { get; set; } = Vector2.one;

        public static Vector2 ScreenSize { get; set; } = Vector2.one;

        public static float LaneScreenHitboxHorizontal => LaneScreenHitboxBase.x * ScreenSize.x / ScreenSizeBase.x;

        public static float LaneScreenHitboxVertical => LaneScreenHitboxBase.y * ScreenSize.y / ScreenSizeBase.y;

        public static bool EnablePauseMenu { get; internal set; } = true;

        public static bool ShouldNotifyOnAudioEnd { get; internal set; } = false;

        public static int RetryCount { get; internal set; } = 0;
    }
}