using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Gameplay.Audio.Practice
{
    public class PracticeTimingControl : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Button backButton;
        [SerializeField] private Button forwardButton;
        
        private void Awake()
        {
            backButton.onClick.AddListener(JumpBack);
            forwardButton.onClick.AddListener(JumpForward);
        }

        private void OnDestroy()
        {
            backButton.onClick.RemoveListener(JumpBack);
            forwardButton.onClick.RemoveListener(JumpForward);
        }

        private void JumpBack()
        {
            int timing = Services.Audio.AudioTiming;
            int duration = JumpDuration(gameplayData.PlaybackSpeed.Value);
            int newTiming = Mathf.Clamp(timing - duration, 0, Services.Audio.AudioLength);
            Services.Audio.Pause();
            Services.Audio.PlayWithDelay(newTiming, Values.DelayBeforeAudioResume);
        }

        private void JumpForward()
        {
            int timing = Services.Audio.AudioTiming;
            int duration = JumpDuration(gameplayData.PlaybackSpeed.Value);
            int newTiming = Mathf.Clamp(timing + duration, 0, Services.Audio.AudioLength);
            Services.Audio.Pause();
            Services.Audio.PlayWithDelay(newTiming, Values.DelayBeforeAudioResume);
        }

        private int JumpDuration(float speed)
        {
            return Mathf.RoundToInt(5000 * speed);
        }
    }
}