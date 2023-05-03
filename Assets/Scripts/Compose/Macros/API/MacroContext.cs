using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Lua;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmySingleton]
    [EmmyAlias("Context")]
    [EmmyGroup("Macros")]
    public class MacroContext
    {
        public static float DropRate => Settings.DropRate.Value;

        public static float Offset => Gameplay.Values.ChartAudioOffset;

        public static float TimingPointDensityFactor => Gameplay.Values.TimingPointDensity;

        public static float BeatlineDensity => Values.BeatlineDensity.Value;

        public static string Language => I18n.CurrentLocale;

        public static float BaseBpm => Gameplay.Values.BaseBpm;

        public static int SongLength => Services.Gameplay.Audio.AudioLength;

        public static int MaxCombo => Services.Gameplay.Chart.NoteCount;

        public static int NoteCount => MaxCombo;

        public static string Title => Services.Project.CurrentChart.Title;

        public static string Composer => Services.Project.CurrentChart.Composer;

        public static string Charter => Services.Project.CurrentChart.Charter;

        public static string Alias => Services.Project.CurrentChart.Alias;

        public static string Difficulty => Services.Project.CurrentChart.Difficulty;

        public static string DifficultyColor => Services.Project.CurrentChart.DifficultyColor;

        public static string ChartPath => Services.Project.CurrentChart.ChartPath;

        public static string Side => Services.Project.CurrentChart.Skin?.Accent ?? "light";

        public static bool IsLight => Side == "light" || Side == "colorless";

        // Constant
        public static int CurrentArcColor => Values.CreateArcColorMode.Value;

        public static int MaxArcColor => (Services.Project.CurrentChart.Colors?.Arc.Count ?? 3) - 1;

        public static string[] AllArcTypes => new string[] { "b", "s", "si", "so", "sisi", "soso", "siso", "sosi" };

        // Editor
        public static string CurrentArcType => Values.CreateArcTypeMode.Value.ToLineTypeString();

        public static bool CurrentIsVoidMode => CurrentIsTraceMode;

        public static bool CurrentIsTraceMode => Values.CreateNoteMode.Value == CreateNoteMode.Trace;

        public static int CurrentTimingGroup => Values.EditingTimingGroup.Value;

        public static int TimingGroupCount => Services.Gameplay.Chart.TimingGroups.Count;

        public static int CurrentTiming
        {
            get => Services.Gameplay.Audio.ChartTiming;
            set
            {
                Services.Gameplay.Audio.ChartTiming = value;
            }
        }

        public static int ScreenWidth => UnityEngine.Screen.width;

        public static int ScreenHeight => UnityEngine.Screen.height;

        public static float ScreenAspectRatio => ScreenWidth / ScreenHeight;

        public static XY ScreenMiddle => new XY(ScreenWidth / 2, ScreenHeight / 2);

        public static float BeatLengthAt(int timing, int timingGroup = 0)
            => 60000 / BpmAt(timing, timingGroup);

        public static float BpmAt(int timing, int timingGroup = 0)
            => Services.Gameplay.Chart.GetTimingGroup(timingGroup).GetBpm(timing);

        public static float DivisorAt(int timing, int timingGroup = 0)
            => Services.Gameplay.Chart.GetTimingGroup(timingGroup).GetDivisor(timing);

        public static double FloorPositionAt(int timing, int timingGroup = 0)
            => Services.Gameplay.Chart.GetTimingGroup(timingGroup).GetFloorPosition(timing);
    }
}