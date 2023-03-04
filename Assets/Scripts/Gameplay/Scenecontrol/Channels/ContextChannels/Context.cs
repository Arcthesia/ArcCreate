using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Class for accessing context value channels")]
    [EmmySingleton]
    public class Context
    {
        public static DropRateChannel DropRate => new DropRateChannel();

        public static GlobalOffsetChannel GlobalOffset => new GlobalOffsetChannel();

        public static CurrentScoreChannel CurrentScore => new CurrentScoreChannel();

        public static CurrentComboChannel CurrentCombo => new CurrentComboChannel();

        public static CurrentTimingChannel CurrentTiming => new CurrentTimingChannel();

        public static ScreenWidthChannel ScreenWidth => new ScreenWidthChannel();

        public static ScreenHeightChannel ScreenHeight => new ScreenHeightChannel();

        public static ProductChannel ScreenAspectRatio => ScreenWidth / ScreenHeight;

        public static ScreenIs16By9Channel Is16By9 => new ScreenIs16By9Channel();

        public static ProductChannel BeatLength(int timingGroup = 0)
            => 60000 / Bpm(timingGroup);

        public static BPMChannel Bpm(int timingGroup = 0)
            => new BPMChannel(timingGroup);

        public static DivisorChannel Divisor(int timingGroup = 0)
            => new DivisorChannel(timingGroup);

        public static FloorPositionChannel FloorPosition(int timingGroup = 0)
            => new FloorPositionChannel(timingGroup);
    }
}