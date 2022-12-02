using System.IO;
using ArcCreate.ChartFormat;
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
        [SerializeField] private AudioClip testAudio;

        public IChartControl Chart => chartService;

        public ISkinControl Skin => skinService;

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
            Chart.AudioClip = testAudio;
            Chart.Play();
        }

        private void Update()
        {
            Services.Audio.UpdateTime();
            Services.Particle.UpdateParticles();
            int currentTiming = Services.Audio.Timing;

            Services.InputFeedback.UpdateInputFeedback();
            Services.Chart.UpdateChart(Services.Audio.Timing);
            Services.Judgement.ProcessInput(Services.Audio.Timing);
        }
    }
}