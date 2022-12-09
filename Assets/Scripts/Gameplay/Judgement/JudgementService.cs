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
        private readonly UnorderedList<LaneTapJudgementRequest> laneTapRequests = new UnorderedList<LaneTapJudgementRequest>(32);
        private readonly UnorderedList<LaneHoldJudgementRequest> laneHoldRequests = new UnorderedList<LaneHoldJudgementRequest>(32);
        private readonly UnorderedList<ArcJudgementRequest> arcRequests = new UnorderedList<ArcJudgementRequest>(32);
        private IInputHandler inputHandler;

        public void Request(LaneTapJudgementRequest request)
        {
            laneTapRequests.Add(request);
        }

        public void Request(LaneHoldJudgementRequest request)
        {
            laneHoldRequests.Add(request);
        }

        public void Request(ArcJudgementRequest request)
        {
            arcRequests.Add(request);
        }

        public void ClearRequests()
        {
            laneTapRequests.Clear();
        }

        public void ProcessInput(int currentTiming)
        {
            PruneExpiredRequests(currentTiming);

            // Manually update input system to minimalize lag
            InputSystem.Update();
            inputHandler.PollInput();
            inputHandler.HandleLaneTapRequests(currentTiming, laneTapRequests);
            inputHandler.HandleLaneHoldRequests(currentTiming, laneHoldRequests);
            inputHandler.HandleArcRequests(currentTiming, arcRequests);
        }

        private void PruneExpiredRequests(int currentTiming)
        {
            for (int i = laneTapRequests.Count - 1; i >= 0; i--)
            {
                var req = laneTapRequests[i];
                if (currentTiming >= req.ExpireAtTiming)
                {
                    req.Receiver.ProcessLaneTapJudgement(currentTiming - req.AutoAtTiming);
                    laneTapRequests.RemoveAt(i);
                }
            }

            for (int i = laneHoldRequests.Count - 1; i >= 0; i--)
            {
                var req = laneHoldRequests[i];
                if (currentTiming >= req.ExpireAtTiming)
                {
                    req.Receiver.ProcessLaneHoldJudgement(currentTiming - req.AutoAtTiming);
                    laneHoldRequests.RemoveAt(i);
                }
            }

            for (int i = arcRequests.Count - 1; i >= 0; i--)
            {
                var req = arcRequests[i];
                if (currentTiming >= req.ExpireAtTiming)
                {
                    req.Receiver.ProcessArcJudgement(currentTiming - req.AutoAtTiming);
                    arcRequests.RemoveAt(i);
                }
            }
        }

        private void Awake()
        {
            Settings.InputMode.OnValueChanged.AddListener(OnInputModeChange);
            OnInputModeChange(Settings.InputMode.Value);
            InputSystem.pollingFrequency = 240;
            EnhancedTouchSupport.Enable();

            Values.LaneScreenHitbox =
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