using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ArcCreate.Selection.Interface
{
    public class OffsetCalibrateDialog : Dialog
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private int[] expectedHitTimings;
        [SerializeField] private TMP_Text[] offsetTexts;
        [SerializeField] private AudioPreview audioPreview;
        private CancellationTokenSource cts = new CancellationTokenSource();

        protected override void Awake()
        {
            base.Awake();
            startButton.onClick.AddListener(StartCalibration);
            cancelButton.onClick.AddListener(CancelCalibration);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            startButton.onClick.RemoveListener(StartCalibration);
            cancelButton.onClick.AddListener(CancelCalibration);
        }

        private void StartCalibration()
        {
            StartCalibrationTask(cts.Token).Forget();
        }

        private void CancelCalibration()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
            audioSource.Stop();
            audioPreview.ResumePreview();
        }

        private async UniTask StartCalibrationTask(CancellationToken ct)
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            audioPreview.StopPreview();

            double dspStartTime = AudioSettings.dspTime + 1;
            double dspEndTime = dspStartTime + audioClip.length;
            audioSource.clip = audioClip;
            audioSource.PlayScheduled(dspStartTime);
            int[] hitTimings = new int[expectedHitTimings.Length];
            SetOffsetTextsState(hitTimings);

            for (int i = 0; i < hitTimings.Length; i++)
            {
                hitTimings[i] = int.MinValue;
            }

            while (AudioSettings.dspTime <= dspStartTime)
            {
                await UniTask.NextFrame();
                if (ct.IsCancellationRequested)
                {
                    return;
                }
            }

            while (AudioSettings.dspTime <= dspEndTime)
            {
                var touches = Touch.activeTouches;
                foreach (var touch in touches)
                {
                    if (touch.began)
                    {
                        int timing = Mathf.RoundToInt((float)(AudioSettings.dspTime - dspStartTime) * 1000);
                        int minDiff = int.MaxValue;
                        int minDiffIndex = 0;
                        for (int i = 0; i < expectedHitTimings.Length; i++)
                        {
                            int diff = Mathf.Abs(expectedHitTimings[i] - timing);
                            if (diff < minDiff)
                            {
                                minDiff = diff;
                                minDiffIndex = i;
                            }
                        }

                        hitTimings[minDiffIndex] = timing;
                        SetOffsetTextsState(hitTimings);
                    }
                }

                await UniTask.NextFrame();
                if (ct.IsCancellationRequested)
                {
                    return;
                }
            }

            int avgOffset = 0;
            for (int i = 0; i < expectedHitTimings.Length; i++)
            {
                int offset = hitTimings[i] - expectedHitTimings[i];
                avgOffset += offset;
            }

            avgOffset /= expectedHitTimings.Length;
            Settings.GlobalAudioOffset.Value = avgOffset;

            Application.targetFrameRate = 60;
            audioSource.Stop();
            audioPreview.ResumePreview();
        }

        private void SetOffsetTextsState(int[] hitTimings)
        {
            for (int i = 0; i < hitTimings.Length; i++)
            {
                int hit = hitTimings[i];
                if (hit == int.MinValue)
                {
                    return;
                }

                int offset = hit - expectedHitTimings[i];
                TMP_Text text = offsetTexts[i];
                text.text = offset.ToString();
            }
        }
    }
}