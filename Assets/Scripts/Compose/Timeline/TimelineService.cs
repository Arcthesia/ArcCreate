using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Timeline
{
    public class TimelineService : MonoBehaviour, ITimelineService
    {
        [SerializeField] private WaveformDisplay waveformDisplay;
        [SerializeField] private Marker timingMarker;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button playReturnButton;
        [SerializeField] private Button stopButton;

        public int ViewFromTiming => waveformDisplay.ViewFromTiming;

        public int ViewToTiming => waveformDisplay.ViewToTiming;

        private bool IsPlaying => Services.Gameplay?.Audio.IsPlaying ?? false;

        private void Awake()
        {
            timingMarker.OnValueChanged += OnTimingMarker;
            waveformDisplay.OnWaveformDrag += OnWaveformDrag;
            pauseButton.onClick.AddListener(Pause);
            playButton.onClick.AddListener(Play);
            playReturnButton.onClick.AddListener(PlayReturn);
            stopButton.onClick.AddListener(Stop);
        }

        private void OnDestroy()
        {
            timingMarker.OnValueChanged -= OnTimingMarker;
            waveformDisplay.OnWaveformDrag -= OnWaveformDrag;
            pauseButton.onClick.RemoveListener(Pause);
            playButton.onClick.RemoveListener(Play);
            playReturnButton.onClick.RemoveListener(PlayReturn);
            stopButton.onClick.RemoveListener(Stop);
        }

        private void Update()
        {
            if (IsPlaying)
            {
                timingMarker.SetTiming(Services.Gameplay.Audio.Timing);
                waveformDisplay.FocusOnTiming(Services.Gameplay.Audio.Timing / 1000f);
            }
        }

        private void Pause()
        {
            Services.Gameplay.Audio.Pause();
            timingMarker.SetTiming(Services.Gameplay.Audio.Timing);
            waveformDisplay.FocusOnTiming(Services.Gameplay.Audio.Timing / 1000f);
        }

        private void Play()
        {
            Services.Gameplay.Audio.ResumeImmediately();
        }

        private void PlayReturn()
        {
            Services.Gameplay.Audio.ResumeReturnableImmediately();
        }

        private void Stop()
        {
            Services.Gameplay.Audio.Stop();
            timingMarker.SetTiming(Services.Gameplay.Audio.Timing);
            waveformDisplay.FocusOnTiming(Services.Gameplay.Audio.Timing / 1000f);
        }

        private void OnWaveformDrag(float x)
        {
            timingMarker.SetDragPosition(x);
        }

        private void OnTimingMarker(Marker marker, int timing)
        {
            Services.Gameplay.Audio.Timing = timing;
            Services.Gameplay.Audio.SetResumeAt(timing);
        }
    }
}