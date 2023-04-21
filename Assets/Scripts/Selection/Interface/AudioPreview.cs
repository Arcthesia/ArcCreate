using System;
using System.Threading;
using ArcCreate.Data;
using ArcCreate.Storage;
using ArcCreate.Storage.Data;
using ArcCreate.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ArcCreate.Selection.Interface
{
    public class AudioPreview : MonoBehaviour
    {
        [SerializeField] private StorageData storage;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float audioFadeDuration = 1;
        [SerializeField] private float switchSceneAudioFadeDuration = 1;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private (LevelStorage level, string audioPath) currentlyPlaying;
        private float minPreviewLength = 5;

        public void StopPreview()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();

            audioSource.DOFade(0, audioFadeDuration).OnComplete(audioSource.Stop);
        }

        public void ResumePreview()
        {
            var (level, chart) = storage.SelectedChart.Value;
            PlayPreviewAudio(level, chart, cts.Token).Forget();
        }

        public async UniTask PlayPreviewAudio(LevelStorage level, ChartSettings chart, CancellationToken ct)
        {
            audioSource.Stop();
            AudioClip clip = await storage.GetAudioClipStreaming(level, chart.AudioPath);
            if (ct.IsCancellationRequested)
            {
                return;
            }

            audioSource.clip = clip;

            float start = Mathf.Clamp(chart.PreviewStart / 1000f, 0, clip.length - minPreviewLength);
            start = Mathf.Max(start, 0);

            float end = Mathf.Clamp(chart.PreviewEnd / 1000f, start + minPreviewLength, clip.length);
            end = Mathf.Min(end, Mathf.Max(clip.length, minPreviewLength));

            float fadeDuration = Mathf.Min(audioFadeDuration, (end - start) / 2);
            if (fadeDuration < 0)
            {
                return;
            }

            while (true)
            {
                audioSource.volume = 0;
                audioSource.time = start;
                audioSource.Play();
                audioSource.DOFade(1, fadeDuration);

                while (audioSource.time < end - fadeDuration)
                {
                    await UniTask.NextFrame();
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }
                }

                audioSource.DOFade(0, fadeDuration);

                while (audioSource.time < end)
                {
                    await UniTask.NextFrame();
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }
                }

                audioSource.Stop();
            }
        }

        private void Awake()
        {
            minPreviewLength = Constants.MinPreviewSegmentLengthMs / 1000f;
            storage.SelectedChart.OnValueChange += OnChartChange;
            storage.OnStorageChange += OnStorageChange;
            storage.OnSwitchToGameplayScene += OnSwitchToGameplayScene;

            if (storage.IsLoaded)
            {
                OnStorageChange();
            }
        }

        private void OnDestroy()
        {
            storage.SelectedChart.OnValueChange -= OnChartChange;
            storage.OnStorageChange -= OnStorageChange;
            storage.OnSwitchToGameplayScene -= OnSwitchToGameplayScene;
            cts.Cancel();
            cts.Dispose();
        }

        private void OnSwitchToGameplayScene()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
            audioSource.DOFade(0, switchSceneAudioFadeDuration).OnComplete(audioSource.Stop);
        }

        private void OnStorageChange()
        {
            OnChartChange(storage.SelectedChart.Value);
        }

        private void OnChartChange((LevelStorage level, ChartSettings chart) obj)
        {
            var (level, chart) = obj;
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();

            if (level != currentlyPlaying.level || chart.AudioPath != currentlyPlaying.audioPath)
            {
                PlayPreviewAudio(level, chart, cts.Token).Forget();
            }

            currentlyPlaying = (level, chart.AudioPath);
        }
    }
}