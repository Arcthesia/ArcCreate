using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class Context
    {
        public static ValueChannel DropRate => new DropRateChannel();

        public static ValueChannel GlobalOffset => new GlobalOffsetChannel();

        public static ValueChannel CurrentScore => new CurrentScoreChannel();

        public static ValueChannel CurrentCombo => new CurrentComboChannel();

        public static ValueChannel CurrentTiming => new CurrentTimingChannel();

        public static ValueChannel ScreenWidth => new ScreenWidthChannel();

        public static ValueChannel ScreenHeight => new ScreenHeightChannel();

        public static ValueChannel ScreenAspectRatio => ScreenWidth / ScreenHeight;

        public static ValueChannel Is16By9 => new ScreenIs16By9Channel();

        public static ValueChannel BeatLength(int timingGroup = 0)
            => 60000 / Bpm(timingGroup);

        public static ValueChannel Bpm(int timingGroup = 0)
            => new BPMChannel(timingGroup);

        public static ValueChannel Divisor(int timingGroup = 0)
            => new DivisorChannel(timingGroup);

        public static ValueChannel FloorPosition(int timingGroup = 0)
            => new FloorPositionChannel(timingGroup);
    }
}