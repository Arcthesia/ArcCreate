using System.Collections;
using System.IO;
using System.Linq;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Audio;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.GameplayCamera;
using ArcCreate.Gameplay.Scenecontrol;
using ArcCreate.Gameplay.Skin;
using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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
        [SerializeField] private CameraService cameraService;
        [SerializeField] private ScenecontrolService scenecontrolService;
        [SerializeField] private AudioClip testAudio;
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Camera backgroundCamera;
        [SerializeField] private Camera overlayCamera;
        [SerializeField] private string testPlayChartFileName = "test_chart.aff";
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Camera arcCamera;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private string testPlayChartDirectory = "";

        public GameObject RemoteListeningHUD;
        public GameObject RemoteReceivingHUD;

        public bool ShouldUpdateInputSystem
        {
            get => Values.ShouldUpdateInputSystem;
            set => Values.ShouldUpdateInputSystem = value;
        }

        public IChartControl Chart => chartService;

        public ISkinControl Skin => skinService;

        public IAudioControl Audio => audioService;

        public ICameraControl Camera => cameraService;

        public IScenecontrolControl Scenecontrol => scenecontrolService;

        public bool IsLoaded =>
            Services.Chart.IsLoaded
            && Services.Scenecontrol.IsLoaded
            && Services.Render.IsLoaded
            && Services.Hitsound.IsLoaded;

        public bool EnablePauseMenu { get => Values.EnablePauseMenu; set => Values.EnablePauseMenu = value; }

        public void SetCameraViewportRect(Rect rect)
        {
            backgroundCamera.rect = rect;
            overlayCamera.rect = rect;
            Values.ScreenSize = backgroundCamera.pixelWidth;
        }

        public void SetCameraEnabled(bool enable)
        {
            backgroundCamera.enabled = enable;
            overlayCamera.enabled = enable;
        }

        public void SetEnableArcDebug(bool enable)
        {
            Services.Judgement.SetDebugDisplayMode(enable);
        }


        public override void OnNoBootScene()
        {
            // Load test chart
            /* string path = Path.Combine(Application.streamingAssetsPath, testPlayChartDirectory);
             if (Application.platform == RuntimePlatform.Android)
             {
                 ImportTestChartAndroid(path).Forget();
             }
             else
             {
                 ImportTestChart(path);
             }

             Settings.InputMode.Value = (int)InputMode.Mouse;
             Services.Judgement.SetDebugDisplayMode(false);
             Services.Scenecontrol.WaitForSceneLoad();
             //Services.Judgement.SetDebugDisplayMode(false);
             */

            // Load test chart
            string path = Path.Combine(Application.streamingAssetsPath, testPlayChartFileName);
            if (Application.platform == RuntimePlatform.Android)
            {
                ImportTestChartAndroid(path).Forget();
            }
            else
            {
                ImportTestChart(path);
            }

            Settings.InputMode.Value = (int)InputMode.Mouse;
            Services.Judgement.SetDebugDisplayMode(false);
            Services.Scenecontrol.WaitForSceneLoad();
        }



        protected override void OnSceneLoad()
        {
            if (Application.platform == RuntimePlatform.Android
             || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Settings.InputMode.Value = (int)InputMode.Touch;
            }

            Time.timeScale = 1;
        }

        /*protected override void OnSceneLoad()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;

            SceneTransitionManager sceneTransitionManager = SceneTransitionManager.Instance;

            //sceneTransitionManager.OldInputSystem.SetActive(false);
            //sceneTransitionManager.NewInputSystem.SetActive(true);

            if (Application.platform == RuntimePlatform.Android
             || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                
                Settings.InputMode.Value = (int)InputMode.Touch;
            }
            else
            {
                Settings.InputMode.Value = (int)InputMode.Mouse;
            }
 
            if(sceneTransitionManager.shouldLoadTestChart)
            {
                sceneTransitionManager.shouldLoadTestChart = false;

                // Load test chart
                testPlayChartDirectory = SceneTransitionManager.Instance.currentTestSongDirectory;
                string path = Path.Combine(Application.streamingAssetsPath, testPlayChartDirectory);
                testPlayChartDirectory = path;
                if (Application.platform == RuntimePlatform.Android)
                {
                    ImportTestChartAndroid(path).Forget();
                }
                else
                {
                    ImportTestChart(path);
                }
          
                Services.Judgement.SetDebugDisplayMode(false);
            }
        }*/

        private async UniTask ImportTestChartAndroid(string path)
        {
            UnityWebRequest www = UnityWebRequest.Get(path);
            await www.SendWebRequest();

            if (!string.IsNullOrWhiteSpace(www.error))
            {
                throw new System.Exception($"Cannot load test chart file");
            }

            byte[] data = www.downloadHandler.data;
            string copyPath = Path.Combine(Application.temporaryCachePath, "test_arc.aff");
            using (FileStream fs = new FileStream(copyPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }

            ImportTestChart(copyPath);
            File.Delete(copyPath);
        }

        private void ImportTestChart(string path)
        {
            string chartPath = Path.Combine(testPlayChartDirectory, "2.aff");
            ChartReader reader = ChartReaderFactory.GetReader(new PhysicalFileAccess(), chartPath);
            reader.Parse();

            string audioPath = Path.Combine(testPlayChartDirectory, "base.wav");
            gameplayData.LoadAudio(audioPath);

            chartService.LoadChart(reader);

            Audio.PlayImmediately(0);
        }

        private void Update()
        {
            if (!IsLoaded)
            {
                return;
            }


            Services.Audio.UpdateTime();
            Services.Particle.UpdateParticles();
            Services.InputFeedback.UpdateInputFeedback();

            int currentTiming = Services.Audio.ChartTiming;

            Services.Chart.UpdateChartJudgement(currentTiming);
            Services.Judgement.ProcessInput(currentTiming);
            Services.Chart.UpdateChartRender(currentTiming);
            Services.Score.UpdateDisplay(currentTiming);
            Services.Camera.UpdateCamera(currentTiming);
            Services.Scenecontrol.UpdateScenecontrol(currentTiming);
            Services.Render.UpdateRenderers();
            gameplayData.NotifyUpdate(currentTiming);
        }
    }
}