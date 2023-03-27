using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ArcCreate.ChartFormat;
using ArcCreate.Data;
using ArcCreate.Gameplay;
using ArcCreate.Remote.Common;
using ArcCreate.Utility.Extension;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ArcCreate.Remote.Gameplay
{
    public class RemoteGameplayControl : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject logDisplay;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private TMP_Text loadingText;
        private IGameplayControl gameplay;
        private IPAddress requestFileFromIP;
        private int requestFileFromPort;
        private MessageChannel channel;
        private readonly Queue<(RemoteControl, byte[])> actionQueue = new Queue<(RemoteControl, byte[])>();
        private bool queueRunning = false;
        private readonly byte[] sendTimingArray = new byte[4];
        private double lastSendTiming = double.MaxValue;

        public void SetGameplay(IGameplayControl gameplay)
        {
            this.gameplay = gameplay;
            canvas.worldCamera = gameplay.Camera.GameplayCamera;
        }

        public void SetTarget(IPAddress requestFileFromIP, int requestFileFromPort, MessageChannel channel)
        {
            Debug.Log(requestFileFromIP);
            this.requestFileFromIP = requestFileFromIP;
            this.requestFileFromPort = requestFileFromPort;
            this.channel = channel;

            lastSendTiming = Time.realtimeSinceStartup + Constants.SyncTimingInterval;
        }

        public async UniTask HandleMessage(RemoteControl control, byte[] data)
        {
            await UniTask.SwitchToMainThread();
            actionQueue.Enqueue((control, data));
            CheckQueue().Forget();
        }

        private async UniTask CheckQueue()
        {
            if (!queueRunning)
            {
                queueRunning = true;
                loadingIndicator.SetActive(actionQueue.Count > 0);

                while (actionQueue.Count > 0)
                {
                    (RemoteControl control, byte[] data) = actionQueue.Dequeue();
                    loadingText.text = I18n.S("Remote.State.Receiving", control);
                    switch (control)
                    {
                        case RemoteControl.CurrentTiming:
                            gameplay.Audio.AudioTiming = GetInt(data);
                            gameplay.Audio.SetResumeAt(gameplay.Audio.AudioTiming);
                            Debug.Log($"Set timing to ({gameplay.Audio.AudioTiming})");
                            break;
                        case RemoteControl.Play:
                            gameplay.Audio.PlayWithDelay(gameplay.Audio.AudioTiming, 200);
                            Debug.Log($"Playing audio");
                            break;
                        case RemoteControl.Pause:
                            gameplay.Audio.Pause();
                            Debug.Log($"Pausing audio");
                            break;

                        case RemoteControl.ReloadAllFiles:
                            Debug.Log($"Starting to reload all files");
                            await RetrieveAllFiles(data);
                            Debug.Log($"Reloading all files completed");
                            break;

                        case RemoteControl.Chart:
                            Debug.Log($"Starting to reload chart file");
                            await RetrieveChart();
                            Debug.Log($"Reloading chart file completed");
                            break;

                        case RemoteControl.Audio:
                            Debug.Log($"Starting to reload audio file");
                            string ext = GetStringASCII(data);
                            await RetrieveAudio(ext);
                            Debug.Log($"Reloading audio file completed");
                            break;

                        case RemoteControl.JacketArt:
                            Debug.Log($"Starting to reload jacket art image file");
                            bool useDefaultJacket = GetBool(data);
                            await RetrieveJacket(useDefaultJacket);
                            Debug.Log($"Reloading jacket art image file completed");
                            break;

                        case RemoteControl.Background:
                            Debug.Log($"Starting to reload background image file");
                            bool useDefaultBackground = GetBool(data);
                            await RetrieveBackground(useDefaultBackground);
                            Debug.Log($"Reloading background image file completed");
                            break;

                        case RemoteControl.Metadata:
                            Debug.Log($"Starting to reload chart metadata file");
                            string chartPath = GetStringASCII(data);
                            await RetrieveMetadata(chartPath);
                            Debug.Log($"Reloading chart metadata completed");
                            break;
                        case RemoteControl.Scenecontrol:
                            Debug.Log($"Starting to reload scenecontrol");
                            await RetrieveScenecontrol();
                            Debug.Log($"Reloading scenecontrol completed");
                            break;
                        case RemoteControl.ShowLog:
                            logDisplay.SetActive(GetBool(data));
                            break;
                    }
                }

                queueRunning = false;
                loadingIndicator.SetActive(actionQueue.Count > 0);
            }
        }

        private async UniTask RetrieveAllFiles(byte[] data)
        {
            string msg = GetStringASCII(data);
            string[] split = msg.Split(',');
            string ext = split[0];

            if (!bool.TryParse(split[1], out bool useDefaultJacket))
            {
                useDefaultJacket = true;
            }

            if (!bool.TryParse(split[2], out bool useDefaultBackground))
            {
                useDefaultBackground = true;
            }

            string chartPath = split[3];

            try
            {
                UniTask chartTask = RetrieveChart();
                UniTask audioTask = RetrieveAudio(ext);
                UniTask jacketTask = RetrieveJacket(useDefaultJacket);
                UniTask bgTask = RetrieveBackground(useDefaultBackground);
                UniTask metadataTask = RetrieveMetadata(chartPath);
                UniTask scTask = RetrieveScenecontrol();
                await UniTask.WhenAll(chartTask, audioTask, jacketTask, bgTask, metadataTask, scTask);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async UniTask RetrieveChart()
        {
            string uri = GetURI("chart");
            using (UnityWebRequest req = UnityWebRequest.Get(uri))
            {
                await req.SendWebRequest();
                if (!string.IsNullOrWhiteSpace(req.error))
                {
                    Debug.LogWarning(I18n.S("Gameplay.Exception.Chart", new Dictionary<string, object>()
                    {
                        { "Path", uri },
                        { "Error", req.error },
                    }));
                    return;
                }

                string chartData = req.downloadHandler.text;
                var reader = new AffChartReader(new VirtualFileAccess(chartData), string.Empty, string.Empty, string.Empty);
                reader.Parse();
                gameplayData.LoadChart(reader, GetURI("sfx/"));
            }
        }

        private async UniTask RetrieveAudio(string ext)
        {
            await gameplayData.LoadAudioFromHttp(GetURI("audio"), ext);
            await UniTask.SwitchToMainThread();
            gameplay.Audio.AudioTiming = Mathf.Clamp(gameplay.Audio.AudioTiming, 0, gameplay.Audio.AudioLength);
        }

        private async UniTask RetrieveJacket(bool useDefault)
        {
            if (useDefault)
            {
                gameplayData.SetDefaultJacket();
            }
            else
            {
                await gameplayData.LoadJacketFromHttp(GetURI("jacket"));
            }
        }

        private async UniTask RetrieveBackground(bool useDefault)
        {
            if (useDefault)
            {
                gameplayData.SetDefaultBackground();
            }
            else
            {
                await gameplayData.LoadBackgroundFromHttp(GetURI("background"));
            }
        }

        private async UniTask RetrieveMetadata(string chartPath)
        {
            string uri = GetURI("metadata");
            using (UnityWebRequest req = UnityWebRequest.Get(uri))
            {
                await req.SendWebRequest();
                if (!string.IsNullOrWhiteSpace(req.error))
                {
                    Debug.LogWarning(I18n.S("Gameplay.Exception.Metadata", new Dictionary<string, object>()
                    {
                        { "Path", uri },
                        { "Error", req.error },
                    }));
                    return;
                }

                string metadata = req.downloadHandler.text;
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(new CamelCaseNamingConvention())
                    .Build();
                ProjectSettings projectSettings = deserializer.Deserialize<ProjectSettings>(metadata);

                foreach (ChartSettings chartSettings in projectSettings.Charts)
                {
                    if (chartSettings.ChartPath == chartPath)
                    {
                        gameplayData.BaseBpm.Value = chartSettings.BaseBpm;
                        gameplayData.Title.Value = chartSettings.Title;
                        gameplayData.Composer.Value = chartSettings.Composer;
                        gameplayData.DifficultyName.Value = chartSettings.Difficulty;

                        if (gameplay != null)
                        {
                            gameplay.Skin.AlignmentSkin = chartSettings.Skin?.Side ?? string.Empty;
                            gameplay.Skin.AccentSkin = chartSettings.Skin?.Accent ?? string.Empty;
                            gameplay.Skin.NoteSkin = chartSettings.Skin?.Note ?? string.Empty;
                            gameplay.Skin.ParticleSkin = chartSettings.Skin?.Particle ?? string.Empty;
                            gameplay.Skin.SingleLineSkin = chartSettings.Skin?.SingleLine ?? string.Empty;
                            gameplay.Skin.TrackSkin = chartSettings.Skin?.Track ?? string.Empty;

                            List<string> arcColor = new List<string>();
                            List<string> arcColorLow = new List<string>();
                            List<Color> finalColor = new List<Color>();
                            List<Color> finalColorLow = new List<Color>();

                            List<Color> defaultArc = gameplay.Skin.DefaultArcColors;
                            List<Color> defaultArcLow = gameplay.Skin.DefaultArcLowColors;
                            Color trace = gameplay.Skin.DefaultTraceColor;
                            Color shadow = gameplay.Skin.DefaultShadowColor;

                            if (chartSettings.Colors != null)
                            {
                                arcColor = chartSettings.Colors.Arc;
                                arcColorLow = chartSettings.Colors.ArcLow;
                                chartSettings.Colors.Trace.ConvertHexToColor(out trace);
                                chartSettings.Colors.Shadow.ConvertHexToColor(out shadow);
                            }

                            int definedColorCount = Mathf.Min(arcColor.Count, arcColorLow.Count);
                            for (int i = 0; i < definedColorCount; i++)
                            {
                                arcColor[i].ConvertHexToColor(out Color high);
                                arcColorLow[i].ConvertHexToColor(out Color low);
                                finalColor.Add(high);
                                finalColorLow.Add(low);
                            }

                            for (int i = definedColorCount; i < defaultArc.Count; i++)
                            {
                                finalColor.Add(defaultArc[i]);
                                finalColorLow.Add(defaultArcLow[i]);
                            }

                            gameplay.Skin.SetTraceColor(trace);
                            gameplay.Skin.SetArcColors(finalColor, finalColorLow);
                            gameplay.Skin.SetShadowColor(shadow);
                        }

                        ColorUtility.TryParseHtmlString(chartSettings.DifficultyColor, out Color c);
                        gameplayData.DifficultyColor.Value = c;

                        bool enableVideoBackground = !string.IsNullOrEmpty(chartSettings.VideoPath);
                        if (enableVideoBackground)
                        {
                            gameplayData.VideoBackgroundUrl.Value = enableVideoBackground ? GetURI("video") : null;
                        }

                        break;
                    }
                }
            }
        }

        private async UniTask RetrieveScenecontrol()
        {
            string uri = GetURI("scjson");
            using (UnityWebRequest req = UnityWebRequest.Get(uri))
            {
                await req.SendWebRequest();
                if (!string.IsNullOrWhiteSpace(req.error))
                {
                    Debug.LogWarning(I18n.S("Gameplay.Exception.Scenecontrol", new Dictionary<string, object>()
                    {
                        { "Path", uri },
                        { "Error", req.error },
                    }));
                    return;
                }

                string json = req.downloadHandler.text;
                gameplay.Scenecontrol.ScenecontrolFolder = GetURI("scenecontrol/");
                gameplay.Scenecontrol.Import(json);
                gameplay.Scenecontrol.WaitForSceneLoad();
            }
        }

        private string GetURI(string path)
        {
            var uri = $"http://{requestFileFromIP}:{requestFileFromPort}/{path}";
            Debug.Log(uri);
            return uri;
        }

        private bool GetBool(byte[] bytes)
        {
            return BitConverter.ToBoolean(bytes, 0);
        }

        private int GetInt(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }

        private string GetStringASCII(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        private void Update()
        {
            if (gameplay == null)
            {
                return;
            }

            int currentTiming = gameplay.Audio.AudioTiming;
            if (Time.realtimeSinceStartup > lastSendTiming + Constants.SyncTimingInterval)
            {
                GetBytes(currentTiming, sendTimingArray);
                channel?.SendMessage(RemoteControl.CurrentTiming, sendTimingArray);

                CheckQueue().Forget();
                lastSendTiming = Time.realtimeSinceStartup;
            }
        }

        // Avoid allocation every frame
        private void GetBytes(int value, byte[] array)
        {
            array[3] = (byte)(value >> 24);
            array[2] = (byte)(value >> 16);
            array[1] = (byte)(value >> 8);
            array[0] = (byte)value;
        }
    }
}