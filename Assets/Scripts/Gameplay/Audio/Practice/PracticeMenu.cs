using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Gameplay.Audio.Practice
{
    public class PracticeMenu : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private PracticeTimeline timeline;

        [Header("Speed")]
        [SerializeField] private Button changeSpeedButton;
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private float speedIncrement;

        [Header("Repeat")]
        [SerializeField] private GameObject repeatOff;
        [SerializeField] private GameObject repeatOn;
        [SerializeField] private Button repeatOffButton;
        [SerializeField] private Button repeatOnButton;
        [SerializeField] private Button repeatFromButton;
        [SerializeField] private Button repeatToButton;
        private int repeatToTiming;
        private int repeatFromTiming;
        private bool repeat;

        private void Awake()
        {
            gameplayData.AudioClip.OnValueChange += OnClipChange;
            if (gameplayData.AudioClip.Value != null)
            {
                OnClipChange(gameplayData.AudioClip.Value);
            }

            changeSpeedButton.onClick.AddListener(ChangeSpeed);
            repeatOffButton.onClick.AddListener(TurnRepeatOff);
            repeatOnButton.onClick.AddListener(TurnRepeatOn);
            repeatFromButton.onClick.AddListener(SetRepeatFrom);
            repeatToButton.onClick.AddListener(SetRepeatTo);
        }

        private void OnDestroy()
        {
            gameplayData.AudioClip.OnValueChange -= OnClipChange;
            gameplayData.OnGameplayUpdate -= CheckRepeat;
            changeSpeedButton.onClick.RemoveListener(ChangeSpeed);
            repeatOffButton.onClick.RemoveListener(TurnRepeatOff);
            repeatOnButton.onClick.RemoveListener(TurnRepeatOn);
            repeatFromButton.onClick.RemoveListener(SetRepeatFrom);
            repeatToButton.onClick.RemoveListener(SetRepeatTo);
        }

        private void OnClipChange(AudioClip clip)
        {
            timeline.LoadWaveformFor(clip);
            repeatFromTiming = 0;
            repeatToTiming = Mathf.RoundToInt(clip.length * 1000);
            UpdateRepeatRange();
        }

        private void ChangeSpeed()
        {
            float speed = gameplayData.PlaybackSpeed.Value;
            float nextSpeed = (Mathf.Floor(speed / speedIncrement) - 1) * speedIncrement;
            if (nextSpeed <= 0)
            {
                nextSpeed = 1;
            }

            speedText.text = nextSpeed.ToString("f2") + "x";
            gameplayData.PlaybackSpeed.Value = nextSpeed;
        }

        private void TurnRepeatOff()
        {
            repeat = false;
            repeatOff.SetActive(true);
            repeatOn.SetActive(false);
            UpdateRepeatRange();
            gameplayData.OnGameplayUpdate -= CheckRepeat;
        }

        private void TurnRepeatOn()
        {
            repeat = true;
            repeatOff.SetActive(false);
            repeatOn.SetActive(true);
            UpdateRepeatRange();
            gameplayData.OnGameplayUpdate += CheckRepeat;
        }

        private void SetRepeatFrom()
        {
            repeatFromTiming = Services.Audio.AudioTiming;
            UpdateRepeatRange();
        }

        private void SetRepeatTo()
        {
            repeatToTiming = Services.Audio.AudioTiming;
            UpdateRepeatRange();
        }

        private void UpdateRepeatRange()
        {
            if (repeatFromTiming > repeatToTiming)
            {
                (repeatFromTiming, repeatToTiming) = (repeatToTiming, repeatFromTiming);
            }

            repeatToTiming = Mathf.Clamp(repeatToTiming, repeatFromTiming + 1000, Services.Audio.AudioLength);
            repeatFromTiming = Mathf.Clamp(repeatFromTiming, 0, repeatToTiming - 1000);
            timeline.SetRepeatRange(repeat, repeatFromTiming, repeatToTiming);
        }

        private void CheckRepeat(int chartTiming)
        {
            int timing = Services.Audio.AudioTiming;
            int length = Services.Audio.AudioLength;
            bool outsideRange = timing < repeatFromTiming - 200 || timing > repeatToTiming;
            bool audioEnd = timing >= length - 100 && repeatToTiming >= length - 100;
            if ((Services.Audio.IsPlaying && outsideRange) || (audioEnd && !gameObject.activeInHierarchy))
            {
                Services.Audio.Pause();
                Services.Audio.PlayWithDelay(repeatFromTiming, 200);
            }
        }
    }
}