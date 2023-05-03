using ArcCreate.Gameplay.Audio.Practice;
using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Gameplay.Audio
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Button pauseButton;
        [SerializeField] private RectTransform pauseButtonRect;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private Button[] playButtons;
        [SerializeField] private Button[] retryButtons;
        [SerializeField] private Button[] returnButtons;
        [SerializeField] private PracticeMenu practiceMenu;
        [SerializeField] private GameObject pauseControl;

        private void Awake()
        {
            pauseButton.onClick.AddListener(OnPauseButton);
            foreach (var playButton in playButtons)
            {
                playButton.onClick.AddListener(OnPlayButton);
            }

            foreach (var retryButton in retryButtons)
            {
                retryButton.onClick.AddListener(OnRetryButton);
            }

            foreach (var returnButton in returnButtons)
            {
                returnButton.onClick.AddListener(OnReturnButton);
            }

            Application.focusChanged += OnFocusChange;
            gameplayData.EnablePracticeMode.OnValueChange += SetPracticeMode;
            SetPracticeMode(gameplayData.EnablePracticeMode.Value);
        }

        private void OnDestroy()
        {
            pauseButton.onClick.RemoveListener(OnPauseButton);
            foreach (var playButton in playButtons)
            {
                playButton.onClick.RemoveListener(OnPlayButton);
            }

            foreach (var retryButton in retryButtons)
            {
                retryButton.onClick.RemoveListener(OnRetryButton);
            }

            foreach (var returnButton in returnButtons)
            {
                returnButton.onClick.RemoveListener(OnReturnButton);
            }

            Application.focusChanged -= OnFocusChange;
            gameplayData.EnablePracticeMode.OnValueChange -= SetPracticeMode;
        }

        private void OnFocusChange(bool focused)
        {
            if (!focused)
            {
                OnPauseButton();
            }
        }

        private void OnPauseButton()
        {
            // Hacky but whatever
            if (Values.EnablePauseMenu
            && (Services.Audio.IsPlayingAndNotStationary || (Services.Audio.AudioTiming >= Services.Audio.AudioLength - 1000)))
            {
                int touchCount = Input.touchCount;
                for (int i = 0; i < touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (!RectTransformUtility.RectangleContainsScreenPoint(pauseButtonRect, touch.position, uiCamera))
                    {
                        return;
                    }
                }

                pauseScreen.SetActive(true);
                Services.Audio.Pause();
            }
        }

        private void OnPlayButton()
        {
            pauseScreen.SetActive(false);
            Services.Audio.ResumeWithDelay(Values.DelayBeforeAudioResume, false);
            Services.Judgement.RefreshInputHandler();
            DisablePauseButton().Forget();
        }

        private void OnRetryButton()
        {
            Values.RetryCount += 1;
            pauseScreen.SetActive(false);
            Services.Judgement.RefreshInputHandler();
            StartRetry().Forget();
        }

        private async UniTask StartRetry()
        {
            ITransition transition = new ShutterTransition(500);
            transition.EnableGameObject();
            await transition.StartTransition();
            Services.Audio.AudioTiming = -Values.DelayBeforeAudioStart;
            await transition.EndTransition();
            if (!pauseScreen.activeInHierarchy)
            {
                Services.Audio.PlayWithDelay(0, Values.DelayBeforeAudioStart);
            }

            await DisablePauseButton();
        }

        private async UniTask DisablePauseButton()
        {
            pauseButton.interactable = false;
            await UniTask.Delay(1000);
            pauseButton.interactable = true;
        }

        private void OnReturnButton()
        {
            SceneTransitionManager.Instance.SetTransition(new ShutterTransition());
            SceneTransitionManager.Instance.SwitchScene(SceneNames.SelectScene).Forget();
        }

        private void SetPracticeMode(bool enable)
        {
            practiceMenu.gameObject.SetActive(enable);
            pauseControl.SetActive(!enable);
        }
    }
}