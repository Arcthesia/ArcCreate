using System;
using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ArcCreate.Gameplay;
using System.Linq.Expressions;

namespace ArcCreate.Gameplay.Audio
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private Button playButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button returnButton;
        public bool inRemote = false;
        public GameplayManager gameplayManager;

        private void Awake()
        {
            pauseButton.onClick.AddListener(OnPauseButton);
            playButton.onClick.AddListener(OnPlayButton);
            retryButton.onClick.AddListener(OnRetryButton);
            returnButton.onClick.AddListener(OnReturnButton);
        }

        private void OnDestroy()
        {
            pauseButton.onClick.RemoveListener(OnPauseButton);
            playButton.onClick.RemoveListener(OnPlayButton);
            retryButton.onClick.RemoveListener(OnRetryButton);
            returnButton.onClick.RemoveListener(OnReturnButton);
        }

        private void OnPauseButton()
        {
            if(inRemote)
            {
                if (gameplayManager.RemoteListeningHUD.activeSelf == true || gameplayManager.RemoteReceivingHUD.activeSelf == true)
                {
                    return;
                }
            }

            if (Values.EnablePauseMenu)
            {
                pauseScreen.SetActive(true);
                Services.Audio.Pause();
            }
        }

        private void OnPlayButton()
        {
            pauseScreen.SetActive(false);
            Services.Audio.ResumeWithDelay(200, false);
        }

        private void OnRetryButton()
        {
            pauseScreen.SetActive(false);
            StartRetry().Forget();
        }

        private async UniTask StartRetry()
        {
            ITransition transition = new ShutterTransition(500);
            transition.EnableGameObject();
            await transition.StartTransition();
            Services.Audio.AudioTiming = 0;
            await transition.EndTransition();
            Services.Audio.PlayWithDelay(0, 200);
        }

        private void OnReturnButton()
        {
            SceneTransitionManager.Instance.SetTransition(new ShutterTransition());
            SceneTransitionManager.Instance.SwitchScene(SceneNames.SelectScene).Forget();
        }
    }
}