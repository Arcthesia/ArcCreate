using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Gameplay.Audio
{
    public class PauseButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const float MaxDoubleClickDuration = 0.2f;
        private const float MinLongPressDuration = 0.5f;
        [SerializeField] private Image image;
        private IPauseButtonHandler pauseButtonHandler;

        public UnityEvent OnActivation { get; } = new UnityEvent();

        public bool Interactable { get; set; } = true;

        public void Activate()
        {
            OnActivation.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pauseButtonHandler.OnClick();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pauseButtonHandler.OnRelease();
        }

        private void Awake()
        {
            Settings.HidePause.OnValueChanged.AddListener(OnHideSettings);
            Settings.PauseButtonMode.OnValueChanged.AddListener(OnModeSettings);
            OnHideSettings(Settings.HidePause.Value);
            OnModeSettings(Settings.PauseButtonMode.Value);
        }

        private void OnDestroy()
        {
            Settings.HidePause.OnValueChanged.RemoveListener(OnHideSettings);
            Settings.PauseButtonMode.OnValueChanged.RemoveListener(OnModeSettings);
        }

        private void OnHideSettings(bool val)
        {
            image.color = new Color(1, 1, 1, val ? 0.01f : 1);
        }

        private void OnModeSettings(int val)
        {
            PauseButtonMode mode = (PauseButtonMode)val;
            switch (mode)
            {
                case PauseButtonMode.ClickOnce:
                    pauseButtonHandler = new SingleClickPauseHandler(this);
                    break;
                case PauseButtonMode.DoubleClick:
                    pauseButtonHandler = new DoubleClickPauseHandler(this, MaxDoubleClickDuration);
                    break;
                case PauseButtonMode.Hold:
                    pauseButtonHandler = new HoldPauseHandler(this, MinLongPressDuration);
                    break;
                case PauseButtonMode.Disable:
                    pauseButtonHandler = new NoOpPauseHandler();
                    break;
            }
        }
    }
}