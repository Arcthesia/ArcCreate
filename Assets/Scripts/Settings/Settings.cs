using UnityEngine;

namespace ArcCreate
{
    public static class Settings
    {
        // Gameplay
        public static readonly IntSetting DropRate = new IntSetting("DropRate", 300);
        public static readonly IntSetting ViewportAspectRatioSetting = new IntSetting("ViewportAspectRatioSetting", 0);
        public static readonly IntSetting InputMode = new IntSetting("Gameplay.InputMode", 0);

        // Audio
        public static readonly IntSetting GlobalAudioOffset = new IntSetting("GlobalAudioOffset", 0);
        public static readonly FloatSetting MusicAudio = new FloatSetting("SoundPreferences.ChartAudio", 0.7f);
        public static readonly FloatSetting EffectAudio = new FloatSetting("SoundPreferences.EffectAudio", 0.7f);

        // Display
        public static readonly IntSetting Framerate = new IntSetting("Framerate", 60);
        public static readonly IntSetting VSync = new IntSetting("VSync", 1);
        public static readonly BoolSetting ShowFPSCounter = new BoolSetting("ShowFrameCounter", false);

        // Input
        public static readonly IntSetting GridSlot = new IntSetting("GridSlot", 0);
        public static readonly FloatSetting ScrollSensitivityVertical =
            new FloatSetting(
                "ScrollSensitivity.Vertical",
                Application.platform == RuntimePlatform.LinuxPlayer
                || Application.platform == RuntimePlatform.LinuxEditor ? -1000 : 10);

        public static readonly FloatSetting ScrollSensitivityHorizontal =
            new FloatSetting(
                "ScrollSensitivity.Horizontal",
                Application.platform == RuntimePlatform.LinuxPlayer
                || Application.platform == RuntimePlatform.LinuxEditor ? 500 : -5);

        public static readonly FloatSetting ScrollSensitivityTimeline =
            new FloatSetting(
                "ScrollSensitivity.Timeline",
                Application.platform == RuntimePlatform.LinuxPlayer
                || Application.platform == RuntimePlatform.LinuxEditor ? -1f : 1f);

        public static readonly FloatSetting TrackScrollThreshold = new FloatSetting("ScrollSensitivity.TrackThreshold", 1);
        public static readonly IntSetting TrackScrollMaxMovement = new IntSetting("ScrollSensitivity.MaxTiming", 200);
        public static readonly FloatSetting CameraSensitivity = new FloatSetting("CameraSensitivity", 10);

        // Export
        public static readonly IntSetting ChartSortMode = new IntSetting("ChartSortMode", 0);
        public static readonly FloatSetting FPS = new FloatSetting("RenderPreferences.FPS", 60);
        public static readonly IntSetting CRF = new IntSetting("RenderPreferences.CRF", 18);
        public static readonly FloatSetting DownscaleFactor = new FloatSetting("RenderPreferences.DownscaleFactor", 1.0f);
        public static readonly StringSetting FFmpegPath = new StringSetting("RenderPreferences.FFmpegPath", "");
        public static readonly BoolSetting EnableEasterEggs = new BoolSetting("Fun.EasterEggs", Application.isEditor);

        [RuntimeInitializeOnLoadMethod]
        public static void OnInitialize()
        {
            Framerate.OnValueChanged.AddListener((value) => Application.targetFrameRate = value);
            Application.targetFrameRate = Framerate.Value;

            VSync.OnValueChanged.AddListener((value) => QualitySettings.vSyncCount = value);
            QualitySettings.vSyncCount = VSync.Value;
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            if (!Application.isEditor)
            {
                PlayerPrefs.Save();
            }
        }
    }
}
