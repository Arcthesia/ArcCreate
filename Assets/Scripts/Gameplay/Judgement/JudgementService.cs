using ArcCreate.Gameplay.Judgement.Input;
using ArcCreate.Utility;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace ArcCreate.Gameplay.Judgement
{
    public class JudgementService : MonoBehaviour, IJudgementService
    {
        [SerializeField] private Camera gameplayCamera;
        private readonly UnorderedList<TapJudgementRequest> tapRequests = new UnorderedList<TapJudgementRequest>(32);
        private IInputHandler inputHandler;

        public void Request(TapJudgementRequest request)
        {
            tapRequests.Add(request);
        }

        public void ClearRequests()
        {
            tapRequests.Clear();
        }

        public void ProcessInput(int currentTiming)
        {
            PruneExpiredRequests(currentTiming);

            // Manually update input system to minimalize lag
            InputSystem.Update();
            inputHandler.PollInput();
            inputHandler.HandleTaps(currentTiming, tapRequests);
        }

        private void PruneExpiredRequests(int currentTiming)
        {
            for (int i = tapRequests.Count - 1; i >= 0; i--)
            {
                var req = tapRequests[i];
                if (currentTiming >= req.ExpireAtTiming)
                {
                    req.Receiver.ProcessJudgement(JudgementResult.LostLate);
                    tapRequests.RemoveAt(i);
                }
            }
        }

        private void Awake()
        {
            Settings.InputMode.OnValueChanged.AddListener(OnInputModeChange);
            OnInputModeChange(Settings.InputMode.Value);
            InputSystem.pollingFrequency = 240;
            EnhancedTouchSupport.Enable();

            Values.TapScreenHitbox =
                (gameplayCamera.WorldToScreenPoint(Vector3.zero).x
               - gameplayCamera.WorldToScreenPoint(new Vector3(Values.LaneWidth, 0, 0)).x)
               / 2;
        }

        private void OnDestroy()
        {
            Settings.InputMode.OnValueChanged.RemoveListener(OnInputModeChange);
            InputSystem.pollingFrequency = 60;
        }

        private void OnInputModeChange(int modeNum)
        {
            InputMode inputMode = (InputMode)modeNum;
            switch (inputMode)
            {
                case InputMode.Auto:
                    inputHandler = new AutoInputHandler();
                    break;
                case InputMode.AutoController:
                    inputHandler = new AutoControllerInputHandler();
                    break;
                case InputMode.Controller:
                    inputHandler = new ControllerInputHandler();
                    break;
                case InputMode.Mouse:
                    inputHandler = new MouseInputHandler();
                    break;
                case InputMode.Touch:
                    inputHandler = new TouchInputHandler();
                    break;
                default:
                    inputHandler = new IdleInputHandler();
                    break;
            }
        }
    }
}