using System;
using ArcCreate.Compose.Components;
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

        private new void Awake()
        {
            base.Awake();
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

        private void OnAudioFile(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnJacket(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnBackground(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnVideo(string obj)
        {
            throw new NotImplementedException();
        }
    }
}