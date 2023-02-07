using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay;
using ArcCreate.Remote.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ArcCreate.Remote.Gameplay
{
    public class RemoteGameplayControl : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Canvas logDisplay;
        private IGameplayControl gameplay;
        private int timingStart = int.MinValue;
        private int timingEnd = int.MaxValue;
        private bool isLooping = false;
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
            logDisplay.worldCamera = gameplay.Camera.GameplayCamera;
        }

        public void SetTarget(IPAddress requestFileFromIP, int requestFileFromPort, MessageChannel channel)
        {
            this.requestFileFromIP = requestFileFromIP;
            this.requestFileFromPort = requestFileFromPort;
            this.channel = channel;

            lastSendTiming = Time.realtimeSinceStartup + Constants.SyncTimingInterval;
        }

        public async UniTask HandleMessage(RemoteControl control, byte[] data)
        {
            await UniTask.SwitchToMainThread();
            switch (control)
            {
                case RemoteControl.CurrentTiming:
                    gameplay.Audio.ChartTiming = GetInt(data);
                    break;
                case RemoteControl.StartTiming:
                    timingStart = GetInt(data);
                    AlignTimingRange();
                    break;
                case RemoteControl.EndTiming:
                    timingEnd = GetInt(data);
                    AlignTimingRange();
                    break;
                case RemoteControl.Loop:
                    isLooping = GetBool(data);
                    break;
                case RemoteControl.Play:
                    gameplay.Audio.ResumeWithDelay(200);
                    break;
                case RemoteControl.Pause:
                    gameplay.Audio.Pause();
                    break;
                case RemoteControl.Chart:
                case RemoteControl.Audio:
                case RemoteControl.JacketArt:
                case RemoteControl.Background:
                    actionQueue.Enqueue((control, data));
                    break;
                case RemoteControl.VideoBackground:
                    bool enabled = GetBool(data);
                    gameplayData.VideoBackgroundUrl.Value = enabled ? GetURI("video") : null;
                    break;
                case RemoteControl.Title:
                    gameplayData.Title.Value = GetStringUTF32(data);
                    break;
                case RemoteControl.Composer:
                    gameplayData.Composer.Value = GetStringUTF32(data);
                    break;
                case RemoteControl.DifficultyName:
                    gameplayData.DifficultyName.Value = GetStringUTF32(data);
                    break;
                case RemoteControl.DifficultyColor:
                    gameplayData.DifficultyColor.Value = GetColor(data);
                    break;
                case RemoteControl.BaseBpm:
                    gameplayData.BaseBpm.Value = GetFloat(data);
                    break;
                case RemoteControl.Speed:
                    Settings.DropRate.Value = GetInt(data);
                    break;
                case RemoteControl.GlobalOffset:
                    Settings.GlobalAudioOffset.Value = GetInt(data);
                    break;
                case RemoteControl.AlignmentSkin:
                    gameplay.Skin.AlignmentSkin = GetStringASCII(data);
                    break;
                case RemoteControl.AccentSkin:
                    gameplay.Skin.AccentSkin = GetStringASCII(data);
                    break;
                case RemoteControl.NoteSkin:
                    gameplay.Skin.NoteSkin = GetStringASCII(data);
                    break;
                case RemoteControl.ParticleSkin:
                    gameplay.Skin.ParticleSkin = GetStringASCII(data);
                    break;
                case RemoteControl.SingleLineSkin:
                    gameplay.Skin.SingleLineSkin = GetStringASCII(data);
                    break;
                case RemoteControl.TrackSkin:
                    gameplay.Skin.TrackSkin = GetStringASCII(data);
                    break;
                case RemoteControl.ShowLog:
                    logDisplay.gameObject.SetActive(GetBool(data));
                    break;
            }

            CheckQueue().Forget();
        }

        private async UniTask CheckQueue()
        {
            if (!queueRunning && actionQueue.Count > 0)
            {
                (RemoteControl control, byte[] data) = actionQueue.Dequeue();
                queueRunning = true;
                switch (control)
                {
                    case RemoteControl.Chart:
                        await RetrieveChart();
                        break;
                    case RemoteControl.Audio:
                        string ext = GetStringASCII(data);
                        await RetrieveAudio(ext);
                        break;
                    case RemoteControl.JacketArt:
                        bool useDefaultJacket = GetBool(data);
                        await RetrieveJacket(useDefaultJacket);
                        break;
                    case RemoteControl.Background:
                        bool useDefaultBackground = GetBool(data);
                        await RetrieveBackground(useDefaultBackground);
                        break;
                }

                queueRunning = false;
                await CheckQueue();
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
                gameplayData.LoadChart(reader);
            }
        }

        private async UniTask RetrieveAudio(string ext)
        {
            await gameplayData.LoadAudioFromHttp(GetURI("audio"), ext);
            await UniTask.SwitchToMainThread();
            timingStart = 0;
            timingEnd = Mathf.RoundToInt(gameplayData.AudioClip.Value.length * 1000);
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

        private string GetURI(string path)
        {
            return $"http://{requestFileFromIP}:{requestFileFromPort}/{path}";
        }

        private bool GetBool(byte[] bytes)
        {
            return BitConverter.ToBoolean(bytes, 0);
        }

        private int GetInt(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }

        private float GetFloat(byte[] bytes)
        {
            return BitConverter.ToSingle(bytes, 0);
        }

        private string GetStringASCII(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        private string GetStringUTF32(byte[] bytes)
        {
            return Encoding.UTF32.GetString(bytes);
        }

        private Color GetColor(byte[] bytes)
        {
            string hex = GetStringASCII(bytes);
            Color c = Color.white;
            ColorUtility.TryParseHtmlString(hex, out c);
            return c;
        }

        private void Update()
        {
            if (gameplay == null)
            {
                return;
            }

            int currentTiming = gameplay.Audio.ChartTiming;
            bool isPlaying = gameplay.Audio.IsPlaying;
            if (currentTiming < timingStart)
            {
                gameplay.Audio.ChartTiming = timingStart;
            }

            if (currentTiming > timingEnd)
            {
                if (isLooping)
                {
                    if (isPlaying)
                    {
                        gameplay.Audio.Pause();
                        gameplay.Audio.PlayWithDelay(timingStart, 200);
                    }
                    else
                    {
                        gameplay.Audio.ChartTiming = timingStart;
                    }
                }
                else if (isPlaying)
                {
                    gameplay.Audio.Pause();
                    gameplay.Audio.ChartTiming = timingEnd;
                }
                else
                {
                    gameplay.Audio.ChartTiming = timingEnd;
                }
            }

            if (Time.realtimeSinceStartup > lastSendTiming + Constants.SyncTimingInterval)
            {
                GetBytes(gameplay.Audio.ChartTiming, sendTimingArray);
                channel?.SendMessage(RemoteControl.CurrentTiming, sendTimingArray);

                lastSendTiming = Time.realtimeSinceStartup;
            }
        }

        private void AlignTimingRange()
        {
            timingStart = Mathf.Min(timingStart, timingEnd - Constants.MinPlaybackRange);
            timingStart = Mathf.Max(timingStart, 0);
            timingEnd = Mathf.Max(timingEnd, timingStart + Constants.MinPlaybackRange);
            if (gameplay != null)
            {
                timingEnd = Mathf.Min(timingEnd, gameplay.Audio.AudioLength);
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