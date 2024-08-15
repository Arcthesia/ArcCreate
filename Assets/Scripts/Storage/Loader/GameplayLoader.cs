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
            await UniTask.DelayFrame(5);
            LoadMetadata(level, chart);
            LoadChart(level, chart);
            LoadScenecontrol(level, chart);

            // Avoid jacket flickering after reload
            UniTask audioTask = LoadAudio(level, chart);
            UniTask bgTask = LoadBackground(level, chart);

            await UniTask.WhenAll(audioTask, bgTask);
            await UniTask.WaitUntil(() => gameplayControl.IsLoaded);
        }

        private async UniTask LoadAudio(LevelStorage level, ChartSettings chart)
        {
            Option<string> audioPath = level.GetRealPath(chart.AudioPath);
            if (!audioPath.HasValue)
            {
                throw new Exception("Audio file does not exist");
            }

            Uri uri = new Uri(audioPath.Value);
            await gameplayData.LoadAudioFromHttp(uri, Path.GetExtension(audioPath.Value));
        }

        private async UniTask LoadBackground(LevelStorage level, ChartSettings chart)
        {
            Option<string> bgPath = level.GetRealPath(chart.BackgroundPath);
            if (!bgPath.HasValue)
            {
                gameplayData.SetDefaultBackground();
                return;
            }

            Uri uri = new Uri(bgPath.Value);
            await gameplayData.LoadBackgroundFromHttp(uri);
        }

        private void LoadScenecontrol(LevelStorage level, ChartSettings chart)
        {
            StorageFileAccessWrapper fileAccess = new StorageFileAccessWrapper(level);
            Option<string> scJsonRealPath = level.GetRealPath(Path.ChangeExtension(chart.ChartPath, ".sc.json"));
            if (scJsonRealPath.HasValue)
            {
                string json = File.ReadAllText(scJsonRealPath.Value);
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
            Option<string> videoPath = chart.VideoPath == null ? null : level.GetRealPath(chart.VideoPath);
            if (enableVideoBackground && videoPath.HasValue)
            {
                gameplayData.LoadVideoBackground(videoPath.Value.Replace("\\", "/"), false);
            }
            else
            {
                gameplayData.LoadVideoBackground(null, false);
            }
        }
    }
}