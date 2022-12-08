using System.IO;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Audio;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Skin;
using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Gameplay
{
    /// <summary>
    /// Gameplay loop.
    /// </summary>
    public class GameplayManager : SceneRepresentative, IGameplayControl
    {
        [SerializeField] private ChartService chartService;
        [SerializeField] private SkinService skinService;
        [SerializeField] private AudioService audioService;
        [SerializeField] private AudioClip testAudio;

        private bool loaded = false;

        public IChartControl Chart => chartService;

        public ISkinControl Skin => skinService;

        public IAudioControl Audio => audioService;

        public override void OnUnloadScene()
        {
            Application.targetFrameRate = 60;
        }

        protected override void OnNoBootScene()
        {
            // Use touch
            Settings.InputMode.Value = (int)InputMode.Touch;

            // Load test chart
            string path = Path.Combine(Application.streamingAssetsPath, "test_tap.aff");
            if (Application.platform == RuntimePlatform.Android)
            {
                ImportTestChartAndroid(path).Forget();
            }
            else
            {
                ImportTestChart(path);
            }
        }

        protected override void OnSceneLoad()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
        }

        private async UniTask ImportTestChartAndroid(string path)
        {
            UnityWebRequest www = UnityWebRequest.Get(path);
            await www.SendWebRequest();

            if (!string.IsNullOrWhiteSpace(www.error))
            {
                throw new System.Exception($"Cannot load test chart file");
            }

            byte[] data = www.downloadHandler.data;
            string copyPath = Path.Combine(Application.temporaryCachePath, "test_tap.aff");
            using (FileStream fs = new FileStream(copyPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }

            ImportTestChart(copyPath);
            File.Delete(copyPath);
        }

        private void ImportTestChart(string path)
        {
            ChartReader reader = ChartReaderFactory.GetReader(new PhysicalFileAccess(), path);
            reader.Parse();
            Chart.LoadChart(reader);
            Audio.AudioClip = testAudio;
            Audio.PlayWithDelay(0, 2000);
            loaded = true;
        }

        private void Update()
        {
            if (!loaded)
            {
                return;
            }

            Services.Audio.UpdateTime();
            Services.Particle.UpdateParticles();
            Services.InputFeedback.UpdateInputFeedback();

            int currentTiming = Services.Audio.Timing;
            Services.Chart.UpdateChart(currentTiming);
            if (Services.Audio.IsPlaying || Values.IsRendering)
            {
                Services.Judgement.ProcessInput(currentTiming);
            }

            Services.Score.UpdateDisplay(currentTiming);
        }
    }
}