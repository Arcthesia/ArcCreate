using System.IO;
using ArcCreate.Compose.Components;
using ArcCreate.Gameplay;
using ArcCreate.Utility.Mp3Converter;
using UnityEngine;

namespace ArcCreate.Compose.Project
{
    public class ChartFilesUI : ChartMetadataUI
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private FileSelectField audioFile;
        [SerializeField] private ImageFileSelectField jacket;
        [SerializeField] private ImageFileSelectField background;
        [SerializeField] private FileSelectField video;

        protected override void ApplyChartSettings(ChartSettings chart)
        {
            audioFile.SetPath(chart.AudioPath);
            jacket.SetPath(chart.JacketPath);
            background.SetPath(chart.BackgroundPath);
            video.SetPath(chart.VideoPath);
        }

        private new void Start()
        {
            base.Start();
            audioFile.OnValueChanged += OnAudioFile;
            jacket.OnValueChanged += OnJacket;
            background.OnValueChanged += OnBackground;
            video.OnValueChanged += OnVideo;
        }

        private void OnDestroy()
        {
            audioFile.OnValueChanged -= OnAudioFile;
            jacket.OnValueChanged -= OnJacket;
            background.OnValueChanged -= OnBackground;
            video.OnValueChanged -= OnVideo;
        }

        private void OnAudioFile(FilePath path)
        {
            if (path == null)
            {
                audioFile.SetPathWithoutNotify(Target.AudioPath);
                return;
            }

            if (Path.GetExtension(path.FullPath) == ".mp3")
            {
                FilePath converted = FilePath.Local(
                    directory: Path.GetDirectoryName(Services.Project.CurrentProject.Path),
                    originalPath: Path.ChangeExtension(path.FullPath, ".wav"));

                Mp3Converter.Mp3ToWav(path.FullPath, converted.FullPath);

                Target.AudioPath = converted.ShortenedPath;
                gameplayData.LoadAudio(converted.FullPath);
                return;
            }

            Target.AudioPath = path.ShortenedPath;
            gameplayData.LoadAudio(path.FullPath);
        }

        private void OnJacket(FilePath path)
        {
            if (path == null)
            {
                Target.JacketPath = null;
                gameplayData.SetDefaultJacket();
                return;
            }

            Target.JacketPath = path.ShortenedPath;
            gameplayData.LoadJacket(path.FullPath);
        }

        private void OnBackground(FilePath path)
        {
            if (path == null)
            {
                Target.BackgroundPath = null;
                gameplayData.SetDefaultBackground();
                return;
            }

            Target.BackgroundPath = path.ShortenedPath;
            gameplayData.LoadBackground(path.FullPath);
        }

        private void OnVideo(FilePath path)
        {
            Target.VideoPath = path.ShortenedPath;
            gameplayData.VideoBackgroundUrl.Value = "file:///" + path.FullPath.Replace("\\", "/");
        }
    }
}