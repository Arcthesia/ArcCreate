using System.IO;
using ArcCreate.Compose.Components;
using ArcCreate.Utility.Mp3Converter;
using UnityEngine;

namespace ArcCreate.Compose.Project
{
    public class ChartFilesUI : ChartMetadataUI
    {
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
                Services.Gameplay.Audio.LoadAudio(converted.FullPath);
                return;
            }

            Target.AudioPath = path.ShortenedPath;
            Services.Gameplay.Audio.LoadAudio(path.FullPath);
        }

        private void OnJacket(FilePath path)
        {
            if (path == null)
            {
                Target.JacketPath = null;
                Services.Gameplay.Skin.SetDefaultJacket();
                return;
            }

            Target.JacketPath = path.ShortenedPath;
            Services.Gameplay.Skin.LoadJacket(path.FullPath);
        }

        private void OnBackground(FilePath path)
        {
            if (path == null)
            {
                Target.BackgroundPath = null;
                Services.Gameplay.Skin.SetDefaultBackground();
            }

            Target.BackgroundPath = path.ShortenedPath;
            Services.Gameplay.Skin.LoadBackground(path.FullPath);
        }

        private void OnVideo(FilePath path)
        {
            Target.VideoPath = path.ShortenedPath;
            Services.Gameplay.Skin.VideoBackgroundUrl = "file:///" + path.FullPath.Replace("\\", "/");
        }
    }
}