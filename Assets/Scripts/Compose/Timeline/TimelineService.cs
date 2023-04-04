using ArcCreate.Compose.Navigation;
using ArcCreate.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Timeline
{
    [EditorScope("Playback")]
    public class TimelineService : MonoBehaviour, ITimelineService
    {
        [SerializeField] private WaveformDisplay waveformDisplay;

        [Header("Markers")]
        [SerializeField] private Marker timingMarker;
        [SerializeField] private Marker returnTimingMarker;

        [Header("Playback control")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pauseHighlight;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject playHighlight;
        [SerializeField] private Button playReturnButton;
        [SerializeField] private GameObject playReturnHighlight;
        [SerializeField] private Button stopButton;

        [Header("Timing navigation")]
        [SerializeField] private int rapidStartingCount = 20;
        [SerializeField] private int rapidDecreaseRate = 5;

        private bool shouldFocusWaveformView = false;

        public int ViewFromTiming => waveformDisplay.ViewFromTiming;

        public int ViewToTiming => waveformDisplay.ViewToTiming;

        private bool IsPlaying => Services.Gameplay?.Audio.IsPlaying ?? false;

        [EditorAction(null, false, "q")]
        [RequireGameplayLoaded]
        public void TogglePlay()
        {
            if (IsPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        [EditorAction("PlayReturn", false, "<space>")]
        [SubAction("Return", false, "<u-space>")]
        [SubAction("Pause", false, "q")]
        [RequireGameplayLoaded]
        public async UniTask StartPlayReturn(EditorAction action)
        {
            SubAction ret = action.GetSubAction("Return");
            SubAction pause = action.GetSubAction("Pause");
            PlayReturn();
            while (true)
            {
                if (ret.WasExecuted)
                {
                    Pause();
                    return;
                }

                if (pause.WasExecuted)
                {
                    Services.Gameplay.Audio.SetReturnOnPause(false);
                    Pause();
                    return;
                }

                await UniTask.NextFrame();
            }
        }

        [EditorAction("ScrollBackOneTick", false, "j")]
        [SubAction("Stop", false, "<u-j>")]
        public async UniTask ScrollBackOneTick(EditorAction action)
        {
            SubAction stop = action.GetSubAction("Stop");
            RapidAction rapidAction = new RapidAction(rapidStartingCount, rapidDecreaseRate);

            while (!stop.WasExecuted)
            {
                if (rapidAction.ShouldExecute())
                {
                    int timing = Services.Gameplay.Audio.ChartTiming;
                    int snap = Services.Grid.MoveTimingBackward(timing);
                    snap = Mathf.Clamp(snap, timing - Settings.TrackScrollMaxMovement.Value, timing + Settings.TrackScrollMaxMovement.Value);
                    Services.Gameplay.Audio.ChartTiming = snap;
                }

                await UniTask.NextFrame();
            }
        }

        [EditorAction("ScrollForwardOneTick", false, "k")]
        [SubAction("Stop", false, "<u-k>")]
        public async UniTask ScrollForwardOneTick(EditorAction action)
        {
            SubAction stop = action.GetSubAction("Stop");
            RapidAction rapidAction = new RapidAction(rapidStartingCount, rapidDecreaseRate);

            while (!stop.WasExecuted)
            {
                if (rapidAction.ShouldExecute())
                {
                    int timing = Services.Gameplay.Audio.ChartTiming;
                    int snap = Services.Grid.MoveTimingForward(timing);
                    snap = Mathf.Clamp(snap, timing - Settings.TrackScrollMaxMovement.Value, timing + Settings.TrackScrollMaxMovement.Value);
                    Services.Gameplay.Audio.ChartTiming = snap;
                }

                await UniTask.NextFrame();
            }
        }

        [EditorAction("ScrollBackwardOneBeat", false, "h")]
        [SubAction("Stop", false, "<u-h>")]
        public async UniTask ScrollBackwardOneBeat(EditorAction action)
        {
            SubAction stop = action.GetSubAction("Stop");
            RapidAction rapidAction = new RapidAction(rapidStartingCount, rapidDecreaseRate);

            while (!stop.WasExecuted)
            {
                if (rapidAction.ShouldExecute())
                {
                    int timing = Services.Gameplay.Audio.ChartTiming;
                    int snap = Services.Grid.MoveTimingBackwardByBeat(timing);
                    Services.Gameplay.Audio.ChartTiming = snap;
                }

                await UniTask.NextFrame();
            }
        }

        [EditorAction("ScrollForwardOneBeat", false, "l")]
        [SubAction("Stop", false, "<u-l>")]
        public async UniTask ScrollForwardOneBeat(EditorAction action)
        {
            SubAction stop = action.GetSubAction("Stop");
            RapidAction rapidAction = new RapidAction(rapidStartingCount, rapidDecreaseRate);

            while (!stop.WasExecuted)
            {
                if (rapidAction.ShouldExecute())
                {
                    int timing = Services.Gameplay.Audio.ChartTiming;
                    int snap = Services.Grid.MoveTimingForwardByBeat(timing);
                    Services.Gameplay.Audio.ChartTiming = snap;
                }

                await UniTask.NextFrame();
            }
        }

        private void Awake()
        {
            timingMarker.OnValueChanged += OnTimingMarker;
            waveformDisplay.OnWaveformDrag += OnWaveformDrag;
            waveformDisplay.OnWaveformScroll += OnWaveformScroll;
            pauseButton.onClick.AddListener(Pause);
            playButton.onClick.AddListener(Play);
            playReturnButton.onClick.AddListener(PlayReturn);
            stopButton.onClick.AddListener(Stop);
        }

        private void OnDestroy()
        {
            timingMarker.OnValueChanged -= OnTimingMarker;
            waveformDisplay.OnWaveformDrag -= OnWaveformDrag;
            waveformDisplay.OnWaveformScroll -= OnWaveformScroll;
            pauseButton.onClick.RemoveListener(Pause);
            playButton.onClick.RemoveListener(Play);
            playReturnButton.onClick.RemoveListener(PlayReturn);
            stopButton.onClick.RemoveListener(Stop);
        }

        private void Update()
        {
            if (Services.Gameplay == null)
            {
                return;
            }

            int currentTiming = Services.Gameplay.Audio.AudioTiming;
            timingMarker.SetTiming(currentTiming);

            if (IsPlaying && shouldFocusWaveformView)
            {
                waveformDisplay.FocusOnTiming(Services.Gameplay.Audio.AudioTiming / 1000f);
            }

            if (!IsPlaying)
            {
                Services.Gameplay.Audio.SetResumeAt(Services.Gameplay.Audio.AudioTiming);
            }
        }

        private void Pause()
        {
            Services.Gameplay.Audio.Pause();
            timingMarker.SetTiming(Services.Gameplay.Audio.AudioTiming);
            waveformDisplay.FocusOnTiming(Services.Gameplay.Audio.AudioTiming / 1000f);
            pauseHighlight.SetActive(true);
            playHighlight.SetActive(false);
            playReturnHighlight.SetActive(false);
        }

        private void Play()
        {
            if (!IsPlaying)
            {
                Services.Gameplay.Audio.ResumeImmediately();
            }
            else
            {
                Services.Gameplay.Audio.SetReturnOnPause(false);
            }

            pauseHighlight.SetActive(false);
            playHighlight.SetActive(true);
            playReturnHighlight.SetActive(false);
            shouldFocusWaveformView = true;
            returnTimingMarker.gameObject.SetActive(false);
        }

        private void PlayReturn()
        {
            if (!IsPlaying)
            {
                Services.Gameplay.Audio.ResumeReturnableImmediately();
            }
            else
            {
                Services.Gameplay.Audio.SetReturnOnPause(true, Services.Gameplay.Audio.AudioTiming);
            }

            pauseHighlight.SetActive(false);
            playHighlight.SetActive(false);
            playReturnHighlight.SetActive(true);
            shouldFocusWaveformView = true;
            returnTimingMarker.gameObject.SetActive(true);
            returnTimingMarker.SetTiming(Services.Gameplay.Audio.AudioTiming);
        }

        private void Stop()
        {
            Services.Gameplay.Audio.Stop();
            pauseHighlight.SetActive(true);
            playHighlight.SetActive(false);
            playReturnHighlight.SetActive(false);
            timingMarker.SetTiming(Services.Gameplay.Audio.AudioTiming);
            waveformDisplay.FocusOnTiming(Services.Gameplay.Audio.AudioTiming / 1000f);
        }

        private void OnWaveformDrag(float x)
        {
            timingMarker.SetDragPosition(x);
            Services.Gameplay.Audio.AudioTiming = timingMarker.Timing;
            Services.Gameplay.Audio.SetResumeAt(timingMarker.Timing);
            shouldFocusWaveformView = false;
        }

        private void OnWaveformScroll()
        {
            shouldFocusWaveformView = false;
        }

        private void OnTimingMarker(Marker marker, int timing)
        {
            Services.Gameplay.Audio.AudioTiming = timing;
            Services.Gameplay.Audio.SetResumeAt(timing);
            shouldFocusWaveformView = false;
        }
    }
}