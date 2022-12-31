using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class FullScreenToggle : MonoBehaviour
    {
        [SerializeField] private Button fullScreenButton;
        [SerializeField] private RectTransform gameplayViewport;
        [SerializeField] private RectTransform fullScreenTransform;
        [SerializeField] private RectTransform defaultTransform;
        [SerializeField] private GameplayViewport viewportResizer;
        [SerializeField] private GameObject fullScreenBackground;
        [SerializeField] private CanvasGroup toggleFullScreenHint;
        [SerializeField] private float hideHintDelay;
        [SerializeField] private float hideHintDuration;
        private Keyboard keyboard;

        private bool isFullScreen = false;

        private void Awake()
        {
            fullScreenButton.onClick.AddListener(ToFullScreen);
            keyboard = InputSystem.GetDevice<Keyboard>();
            fullScreenBackground.SetActive(false);
        }

        private void OnDestroy()
        {
            fullScreenButton.onClick.RemoveListener(ToFullScreen);
        }

        private void Update()
        {
            // TODO: TEMPORARY UNTIL A PROPER KEYBIND SYSTEM IS IMPLEMENTED
            if (keyboard.f11Key.wasPressedThisFrame)
            {
                if (isFullScreen)
                {
                    ToDefault();
                }
                else
                {
                    ToFullScreen();
                }
            }
        }

        private void ToFullScreen()
        {
            gameplayViewport.SetParent(fullScreenTransform, false);
            toggleFullScreenHint.alpha = 1;
            toggleFullScreenHint.DOFade(0, hideHintDuration).SetDelay(hideHintDelay);
            viewportResizer.ResizeNow();
            fullScreenBackground.SetActive(true);
            isFullScreen = true;
        }

        private void ToDefault()
        {
            gameplayViewport.SetParent(defaultTransform, false);
            viewportResizer.ResizeNow();
            toggleFullScreenHint.DOKill();
            toggleFullScreenHint.alpha = 0;
            fullScreenBackground.SetActive(false);
            isFullScreen = false;
        }
    }
}