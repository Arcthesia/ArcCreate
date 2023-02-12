using ArcCreate.Compose.Components;
using ArcCreate.Data;
using ArcCreate.Gameplay;
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
            audioFile.SetPathWithoutNotify(chart.AudioPath);
            jacket.SetPathWithoutNotify(chart.JacketPath);
            background.SetPathWithoutNotify(chart.BackgroundPath);
            video.SetPathWithoutNotify(chart.VideoPath);

            GameplayData.LoadAudio(audioFile.CurrentPath.FullPath);
            GameplayData.LoadJacket(jacket.CurrentPath.FullPath);
            GameplayData.LoadBackground(background.CurrentPath.FullPath);

            if (video.CurrentPath != null)
            {
                GameplayData.VideoBackgroundUrl.Value = "file:///" + video.CurrentPath.FullPath.Replace("\\", "/");
            }
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

            Target.AudioPath = path.ShortenedPath;
            GameplayData.LoadAudio(path.FullPath);

            Values.ProjectModified = true;
        }

        private void OnJacket(FilePath path)
        {
            if (path == null)
            {
                Target.JacketPath = null;
                GameplayData.SetDefaultJacket();
                return;
            }

            Target.JacketPath = path.ShortenedPath;
            GameplayData.LoadJacket(path.FullPath);

            Values.ProjectModified = true;
        }

        private void OnBackground(FilePath path)
        {
            if (path == null)
            {
                Target.BackgroundPath = null;
                GameplayData.SetDefaultBackground();
                return;
            }

            Target.BackgroundPath = path.ShortenedPath;
            GameplayData.LoadBackground(path.FullPath);

            Values.ProjectModified = true;
        }

        private void OnVideo(FilePath path)
        {
            Target.VideoPath = path.ShortenedPath;
            GameplayData.VideoBackgroundUrl.Value = "file:///" + path.FullPath.Replace("\\", "/");

            Values.ProjectModified = true;
        }
    }
}