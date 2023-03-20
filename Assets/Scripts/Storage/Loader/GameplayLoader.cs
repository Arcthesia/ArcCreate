using System;
using System.Collections.Generic;
using System.IO;
using ArcCreate.ChartFormat;
using ArcCreate.Data;
using ArcCreate.Gameplay;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Extension;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Storage
{
    public class GameplayLoader
    {
        private readonly IGameplayControl gameplayControl;
        private readonly GameplayData gameplayData;

        public GameplayLoader(IGameplayControl gameplayControl, GameplayData gameplayData)
        {
            this.gameplayControl = gameplayControl;
            this.gameplayData = gameplayData;
        }

        public async UniTask Load(LevelStorage level, ChartSettings chart)
        {
            LoadMetadata(level, chart);
            LoadChart(level, chart);
            LoadScenecontrol(level, chart);

            UniTask jacketTask = LoadJacket(level, chart);
            UniTask audioTask = LoadAudio(level, chart);
            UniTask bgTask = LoadBackground(level, chart);

            await UniTask.WhenAll(audioTask, jacketTask, bgTask);
            await UniTask.WaitUntil(() => gameplayControl.IsLoaded);
        }

        private async UniTask LoadAudio(LevelStorage level, ChartSettings chart)
        {
            string audioPath = level.GetRealPath(chart.AudioPath);
            string uri = "file:///" + Uri.EscapeDataString(audioPath.Replace("\\", "/"));
            await gameplayData.LoadAudioFromHttp(uri, System.IO.Path.GetExtension(audioPath));
        }

        private async UniTask LoadJacket(LevelStorage level, ChartSettings chart)
        {
            string jacketPath = level.GetRealPath(chart.JacketPath);
            if (string.IsNullOrEmpty(jacketPath))
            {
                gameplayData.SetDefaultJacket();
                return;
            }

            string uri = "file:///" + Uri.EscapeDataString(jacketPath.Replace("\\", "/"));
            await gameplayData.LoadJacketFromHttp(uri);
        }

        private async UniTask LoadBackground(LevelStorage level, ChartSettings chart)
        {
            string bgPath = level.GetRealPath(chart.BackgroundPath);
            if (string.IsNullOrEmpty(bgPath))
            {
                gameplayData.SetDefaultBackground();
                return;
            }

            string uri = "file:///" + Uri.EscapeDataString(bgPath.Replace("\\", "/"));
            await gameplayData.LoadBackgroundFromHttp(uri);
        }

        private void LoadScenecontrol(LevelStorage level, ChartSettings chart)
        {
            StorageFileAccessWrapper fileAccess = new StorageFileAccessWrapper(level);
            string scJsonPath = Path.GetFileNameWithoutExtension(chart.ChartPath) + ".sc.json";
            string scJsonRealPath = level.GetRealPath(scJsonPath);
            if (scJsonRealPath != null)
            {
                string json = File.ReadAllText(scJsonRealPath);
                gameplayControl.Scenecontrol.Import(json, fileAccess);
            }

            gameplayControl.Scenecontrol.WaitForSceneLoad();
        }

        private void LoadChart(LevelStorage level, ChartSettings chart)
        {
            StorageFileAccessWrapper fileAccess = new StorageFileAccessWrapper(level);
            ChartReader reader = ChartReaderFactory.GetReader(fileAccess, chart.ChartPath);
            reader.Parse();
            gameplayData.LoadChart(reader, "", fileAccess);
        }

        private void LoadMetadata(LevelStorage level, ChartSettings chart)
        {
            gameplayData.BaseBpm.Value = chart.BaseBpm;
            gameplayData.Title.Value = chart.Title;
            gameplayData.Composer.Value = chart.Composer;
            gameplayData.DifficultyName.Value = chart.Difficulty;

            gameplayControl.Skin.AlignmentSkin = chart.Skin?.Side ?? string.Empty;
            gameplayControl.Skin.AccentSkin = chart.Skin?.Accent ?? string.Empty;
            gameplayControl.Skin.NoteSkin = chart.Skin?.Note ?? string.Empty;
            gameplayControl.Skin.ParticleSkin = chart.Skin?.Particle ?? string.Empty;
            gameplayControl.Skin.SingleLineSkin = chart.Skin?.SingleLine ?? string.Empty;
            gameplayControl.Skin.TrackSkin = chart.Skin?.Track ?? string.Empty;

            List<string> arcColor = new List<string>();
            List<string> arcColorLow = new List<string>();
            List<Color> finalColor = new List<Color>();
            List<Color> finalColorLow = new List<Color>();

            List<Color> defaultArc = gameplayControl.Skin.DefaultArcColors;
            List<Color> defaultArcLow = gameplayControl.Skin.DefaultArcLowColors;
            Color trace = gameplayControl.Skin.DefaultTraceColor;
            Color shadow = gameplayControl.Skin.DefaultShadowColor;

            if (chart.Colors != null)
            {
                arcColor = chart.Colors.Arc;
                arcColorLow = chart.Colors.ArcLow;
                chart.Colors.Trace.ConvertHexToColor(out trace);
                chart.Colors.Shadow.ConvertHexToColor(out shadow);
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

            gameplayControl.Skin.SetTraceColor(trace);
            gameplayControl.Skin.SetArcColors(finalColor, finalColorLow);
            gameplayControl.Skin.SetShadowColor(shadow);

            ColorUtility.TryParseHtmlString(chart.DifficultyColor, out Color c);
            gameplayData.DifficultyColor.Value = c;

            bool enableVideoBackground = !string.IsNullOrEmpty(chart.VideoPath);
            string videoPath = chart.VideoPath == null ? null : level.GetRealPath(chart.VideoPath);
            if (enableVideoBackground && videoPath != null)
            {
                string videoUri = "file:///" + Uri.EscapeDataString(videoPath.Replace("\\", "/"));
                gameplayData.VideoBackgroundUrl.Value = videoUri;
            }
            else
            {
                gameplayData.VideoBackgroundUrl.Value = null;
            }
        }
    }
}