using UnityEngine;

namespace ArcCreate
{
    public class Settings : MonoBehaviour
    {
        public static readonly IntSetting Framerate = new IntSetting("Framerate", 60);
        public static readonly IntSetting VSync = new IntSetting("VSync", 1);
        public static readonly BoolSetting ShowFPSCounter = new BoolSetting("ShowFrameCounter", false);
        public static readonly BoolSetting AudioSync = new BoolSetting("AudioSync", false);
        public static readonly BoolSetting EditorAuto = new BoolSetting("Auto", true);
        public static readonly IntSetting DropRate = new IntSetting("DropRate", 150);
        public static readonly IntSetting ChartSortMode = new IntSetting("ChartSortMode", 0);
        public static readonly StringSetting X = new StringSetting("CustomGrid.X", "");
        public static readonly StringSetting Y = new StringSetting("CustomGrid.Y", "");
        public static readonly IntSetting LaneFrom = new IntSetting("CustomGrid.LaneFrom", 1);
        public static readonly IntSetting LaneTo = new IntSetting("CustomGrid.LaneTo", 4);
        public static readonly BoolSetting ScalingGrid = new BoolSetting("CustomGrid.Scaling", true);
        public static readonly IntSetting MaxBeatlineCount = new IntSetting("CustomGrid.MaxBeatlineCount", 1000);
        public static readonly FloatSetting FPS = new FloatSetting("RenderPreferences.FPS", 60);
        public static readonly IntSetting CRF = new IntSetting("RenderPreferences.CRF", 18);
        public static readonly FloatSetting DownscaleFactor = new FloatSetting("RenderPreferences.DownscaleFactor", 1.0f);
        public static readonly StringSetting FFmpegPath = new StringSetting("RenderPreferences.FFmpegPath", "");
        public static readonly FloatSetting MusicAudio = new FloatSetting("SoundPreferences.ChartAudio", 0.7f);
        public static readonly FloatSetting EffectAudio = new FloatSetting("SoundPreferences.EffectAudio", 0.7f);
        public static readonly IntSetting InputMode = new IntSetting("Gameplay.InputMode", 0);

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Framerate.OnValueChanged.AddListener((value) => Application.targetFrameRate = value);
            Application.targetFrameRate = Framerate.Value;

            VSync.OnValueChanged.AddListener((value) => QualitySettings.vSyncCount = value);
            QualitySettings.vSyncCount = VSync.Value;
        }

        private void OnDestroy()
        {
            PlayerPrefs.Save();
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.Save();
        }
    }
}
