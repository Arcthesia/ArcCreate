using ArcCreate.Gameplay.Audio.Practice;
using ArcCreate.SceneTransition;
using ArcCreate.Utility.Extension;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Gameplay.Audio
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private StringSO retryCount;
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
        private TransitionSequence retryTransition;

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

            retryTransition = new TransitionSequence()
                .OnShow()
                .AddTransition(new SoundTransition(TransitionScene.Sound.Retry))
                .OnBoth()
                .AddTransition(new TriangleTileTransition())
                .AddTransition(new PlayRetryCountTransition())
                .AddTransition(new DecorationTransition());
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
            retryCount.Value = TextFormat.FormatRetryCount(Values.RetryCount + 1);
            pauseScreen.SetActive(false);
            Services.Judgement.RefreshInputHandler();
            StartRetry().Forget();
        }

        private async UniTask StartRetry()
        {
            await retryTransition.Show();
            Services.Audio.AudioTiming = -Values.DelayBeforeAudioStart;
            await retryTransition.Hide();
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
            TransitionSequence transition = new TransitionSequence()
                .OnShow()
                .AddTransition(new TriangleTileTransition())
                .OnBoth()
                .AddTransition(new DecorationTransition());
            SceneTransitionManager.Instance.SetTransition(transition);
            SceneTransitionManager.Instance.SwitchScene(SceneNames.SelectScene).Forget();
        }

        private void SetPracticeMode(bool enable)
        {
            practiceMenu.gameObject.SetActive(enable);
            pauseControl.SetActive(!enable);
        }
    }
}