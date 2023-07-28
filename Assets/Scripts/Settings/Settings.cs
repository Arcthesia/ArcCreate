using System.Globalization;
using UnityEngine;

namespace ArcCreate
{
    public static class Settings
    {
        public static readonly StringSetting Locale = new StringSetting("Locale", "en_us");

        // Gameplay
        public static readonly IntSetting DropRate = new IntSetting("DropRate", 150);
        public static readonly BoolSetting ShowEarlyLatePerfect = new BoolSetting("ShowEarlyLate", true);
        public static readonly BoolSetting EnableColorblind = new BoolSetting("EnableColorblind", false);
        public static readonly IntSetting FrPmIndicatorPosition = new IntSetting(
            "IndicatorPosition",
            (int)(Application.isMobilePlatform ? FrPmPosition.Middle : FrPmPosition.Off));

        public static readonly BoolSetting EnableMaxIndicator = new BoolSetting("EnableMaxIndicator", false);
        public static readonly IntSetting LateEarlyTextPosition = new IntSetting("LateEarlyTextPosition", 0);
        public static readonly IntSetting ViewportAspectRatioSetting = new IntSetting("ViewportAspectRatioSetting", 0);
        public static readonly BoolSetting ShowGameplayDebug = new BoolSetting("ShowGameplayDebug", false);
        public static readonly IntSetting InputMode = new IntSetting("Gameplay.InputMode", 0);

        // Audio
        public static readonly IntSetting GlobalAudioOffset = new IntSetting("GlobalAudioOffset", 0);
        public static readonly FloatSetting MusicAudio = new FloatSetting("SoundPreferences.ChartAudio", 1f);
        public static readonly FloatSetting EffectAudio = new FloatSetting("SoundPreferences.EffectAudio", 0.4f);

        // Display
        public static readonly IntSetting Framerate = new IntSetting("DisplayFramerate", -1);
        public static readonly BoolSetting VSync = new BoolSetting("EnableVSync", false);
        public static readonly BoolSetting LimitFrameRate = new BoolSetting("LimitFrameRate", false);
        public static readonly BoolSetting ShowFPSCounter = new BoolSetting("ShowFrameCounter", false);

        // Input
        public static readonly IntSetting GridSlot = new IntSetting("GridSlot", 0);
        public static readonly FloatSetting ScrollSensitivityVertical = new FloatSetting("Scroll.Vertical", 200);
        public static readonly FloatSetting ScrollSensitivityHorizontal = new FloatSetting("Scroll.Hozirontal", 100);
        public static readonly FloatSetting ScrollSensitivityTimeline = new FloatSetting("Scroll.Timeline", 0.2f);

        public static readonly FloatSetting TrackScrollThreshold = new FloatSetting("Scroll.TrackThreshold", 0.2f);
        public static readonly IntSetting TrackScrollMaxMovement = new IntSetting("Scroll.MaxTiming", 200);
        public static readonly FloatSetting CameraSensitivity = new FloatSetting("CameraSensitivity", 10);
        public static readonly FloatSetting GridBpmLimit = new FloatSetting("GridBpmLimit", 1000);
        public static readonly BoolSetting ScenecontrolAutoRebuild = new BoolSetting("ScenecontrolAutoRebuild", false);

        // Export
        public static readonly IntSetting ChartSortMode = new IntSetting("ChartSortMode", 0);
        public static readonly FloatSetting FPS = new FloatSetting("RenderPreferences.FPS", 60);
        public static readonly IntSetting CRF = new IntSetting("RenderPreferences.CRF", 25);
        public static readonly IntSetting RenderWidth = new IntSetting("RenderPreferences.Width", 1920);
        public static readonly IntSetting RenderHeight = new IntSetting("RenderPreferences.Height", 1080);
        public static readonly FloatSetting DownscaleFactor = new FloatSetting("RenderPreferences.DownscaleFactor", 1.0f);
        public static readonly StringSetting FFmpegPath = new StringSetting("RenderPreferences.FFmpegPath", "ffmpeg");
        public static readonly BoolSetting EnableEasterEggs = new BoolSetting("Fun.EasterEggs", Application.isEditor);

        // Selection
        public static readonly StringSetting SelectionGroupStrategy = new StringSetting("Selection.Group", "none");
        public static readonly StringSetting SelectionSortStrategy = new StringSetting("Selection.Sort", "title");

        // Editor
        public static readonly BoolSetting ShouldAutosave = new BoolSetting("Editor.Autosave.Enable", true);
        public static readonly IntSetting AutosaveInterval = new IntSetting("Editor.Autosave.Interval", 300);
        public static readonly BoolSetting ShouldBackup = new BoolSetting("Editor.Backup.Enable", true);
        public static readonly IntSetting BackupCount = new IntSetting("Editor.Backup.Count", 10);
        public static readonly BoolSetting SyncToDSPTime = new BoolSetting("Editor.SyncToDSPTime", false);

        public static readonly StringSetting LastUsedPublisherName = new StringSetting("Editor.Export.LastUsedPublisherName", null);

        [RuntimeInitializeOnLoadMethod]
        public static void OnInitialize()
        {
            if (Application.isMobilePlatform)
            {
                LimitFrameRate.OnValueChanged.AddListener((value) => Application.targetFrameRate = value ? 60 : Screen.currentResolution.refreshRate);
                Application.targetFrameRate = LimitFrameRate.Value ? 60 : Screen.currentResolution.refreshRate;

                QualitySettings.vSyncCount = 0;
            }
            else
            {
                Framerate.OnValueChanged.AddListener((value) => Application.targetFrameRate = value);
                Application.targetFrameRate = Framerate.Value;

                VSync.OnValueChanged.AddListener((value) => QualitySettings.vSyncCount = value ? 1 : 0);
                QualitySettings.vSyncCount = VSync.Value ? 1 : 0;
            }

            Application.quitting += OnApplicationQuit;
            CultureInfo.CurrentCulture = new CultureInfo("en");
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
