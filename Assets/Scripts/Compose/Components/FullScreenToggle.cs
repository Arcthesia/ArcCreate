using ArcCreate.Compose.Navigation;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    [EditorScope("Display")]
    public class FullScreenToggle : MonoBehaviour
    {
        [SerializeField] private Button fullScreenButton;
        [SerializeField] private RectTransform gameplayViewport;
        [SerializeField] private RectTransform fullScreenTransform;
        [SerializeField] private RectTransform defaultTransform;
        [SerializeField] private GameplayViewport viewportResizer;
        [SerializeField] private GameObject fullScreenBackground;
        [SerializeField] private CanvasGroup toggleFullScreenHint;
        [SerializeField] private GameObject[] forceCloseOnFullScreen;
        [SerializeField] private float hideHintDelay;
        [SerializeField] private float hideHintDuration;

        [EditorAction("FullScreen", true, "<f11>")]
        [KeybindHint(Exclude = true)]
        public void Toggle()
        {
            if (Values.FullScreen.Value)
            {
                ToDefault();
            }
            else
            {
                ToFullScreen();
            }
        }

        private void Awake()
        {
            fullScreenButton.onClick.AddListener(ToFullScreen);
            fullScreenBackground.SetActive(false);
        }

        private void OnDestroy()
        {
            fullScreenButton.onClick.RemoveListener(ToFullScreen);
        }

        private void ToFullScreen()
        {
            gameplayViewport.SetParent(fullScreenTransform, false);
            toggleFullScreenHint.alpha = 1;
            toggleFullScreenHint.DOFade(0, hideHintDuration).SetDelay(hideHintDelay);
            fullScreenBackground.SetActive(true);
            Values.FullScreen.Value = true;
            foreach (var gameObject in forceCloseOnFullScreen)
            {
                if (gameObject.TryGetComponent<Dialog>(out var dialog))
                {
                    dialog.Close();
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void ToDefault()
        {
            gameplayViewport.SetParent(defaultTransform, false);
            toggleFullScreenHint.DOKill();
            toggleFullScreenHint.alpha = 0;
            fullScreenBackground.SetActive(false);
            Values.FullScreen.Value = false;
        }
    }
}