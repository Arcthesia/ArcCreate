using System;
using System.IO;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose
{
    public static class Values
    {
        public const float EditorBeatlineThickness = 30f;
        public const float TapOverlapWarningThreshold = 2f;

        // Extensions
        public const string ProjectExtension = ".arcproj";
        public const string ProjectExtensionWithoutDot = "arcproj";

        public static readonly string[] ImageExtensions
            = new string[] { ".jpg", ".png" };

        public static readonly string[] AudioExtensions
            = new string[] { ".ogg", ".wav", ".mp3" };

        // Names
        public const string DefaultChartFileName = "2";
        public const string DefaultBpm = "100";
        public const string BaseFileName = "base";
        public const string BackgroundFileName = "bg";
        public const string BackgroundFilePrefix = "bg_";
        public const string DefaultTitle = "Untitled";
        public const string DefaultComposer = "N/A";
        public const string NavigationI18nPrefix = "Compose.Navigation.Actions";
        public const string KeybindSettingsFileName = "keybind";

        // Pools
        public const string TickPoolName = "TickPool";
        public const string BeatlinePoolName = "EditorBeatlinePool";
        public const string ColliderPoolName = "NoteColliderPool";

        // States
        public static State<int> EditingTimingGroup { get; } = new State<int>();

        public static State<float> BeatlineDensity { get; } = new State<float>(4);

        public static State<bool> FullScreen { get; } = new State<bool>();

        public static State<CreateNoteMode> CreateNoteMode { get; } = new State<CreateNoteMode>(Compose.CreateNoteMode.Idle);

        public static State<ArcLineType> CreateArcTypeMode { get; } = new State<ArcLineType>(ArcLineType.S);

        public static State<int> CreateArcColorMode { get; } = new State<int>(0);

        public static Action OnEditAction { get; set; }

        public static float LaneFromX { get; set; } = -8.5f;

        public static float LaneToX { get; set; } = 8.5f;

        public static bool ProjectModified { get; set; } = false;

        public static string ScenecontrolFolder
            => Path.Combine(Path.GetDirectoryName(Services.Project.CurrentProject.Path), "Scenecontrol");
    }
}