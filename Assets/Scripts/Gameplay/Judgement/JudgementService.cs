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
        [SerializeField] private Transform skyInput;
        [SerializeField] private JudgementDebug debug;
        private readonly UnorderedList<LaneTapJudgementRequest> laneTapRequests = new UnorderedList<LaneTapJudgementRequest>(32);
        private readonly UnorderedList<LaneHoldJudgementRequest> laneHoldRequests = new UnorderedList<LaneHoldJudgementRequest>(32);
        private readonly UnorderedList<ArcJudgementRequest> arcRequests = new UnorderedList<ArcJudgementRequest>(32);
        private readonly UnorderedList<ArcTapJudgementRequest> arcTapRequests = new UnorderedList<ArcTapJudgementRequest>(32);
        private IInputHandler inputHandler;
        private bool isAuto;

        public float SkyInputY => skyInput.position.y;

        public IJudgementDebug Debug { get; private set; } = new NoOpJudgementDebug();

        public void SetDebugDisplayMode(bool display)
        {
            if (display)
            {
                Debug = debug;
            }
            else
            {
                Debug = new NoOpJudgementDebug();
            }

            debug.gameObject.SetActive(display);
        }

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

        public void Request(ArcTapJudgementRequest request)
        {
            arcTapRequests.Add(request);
        }

        public void ResetJudge()
        {
            laneTapRequests.Clear();
            laneHoldRequests.Clear();
            arcRequests.Clear();
            arcTapRequests.Clear();
            inputHandler.ResetJudge();
        }

        public void ProcessInput(int currentTiming)
        {
            // Manually update input system to minimalize lag
            if (Values.ShouldUpdateInputSystem)
            {
                InputSystem.Update();
            }

            if (!Services.Audio.IsPlayingAndNotStationary)
            {
                return;
            }

            if (!isAuto)
            {
                PruneExpiredRequests(currentTiming);
            }

            inputHandler.PollInput();
            inputHandler.HandleTapRequests(currentTiming, laneTapRequests, arcTapRequests);
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
                    req.Receiver.ProcessLaneHoldJudgement(true, req.IsJudgement);
                    laneHoldRequests.RemoveAt(i);
                }
            }

            for (int i = arcRequests.Count - 1; i >= 0; i--)
            {
                var req = arcRequests[i];
                if (currentTiming >= req.ExpireAtTiming)
                {
                    req.Receiver.ProcessArcJudgement(true, req.IsJudgement);
                    arcRequests.RemoveAt(i);
                }
            }

            for (int i = arcTapRequests.Count - 1; i >= 0; i--)
            {
                var req = arcTapRequests[i];
                if (currentTiming >= req.ExpireAtTiming)
                {
                    req.Receiver.ProcessArcTapJudgement(currentTiming - req.AutoAtTiming);
                    arcTapRequests.RemoveAt(i);
                }
            }
        }

        private void Awake()
        {
            Settings.InputMode.OnValueChanged.AddListener(OnInputModeChange);
            OnInputModeChange(Settings.InputMode.Value);
            InputSystem.pollingFrequency = 240;
            EnhancedTouchSupport.Enable();

            Values.LaneScreenHitboxBase =
                (gameplayCamera.WorldToScreenPoint(Vector3.zero).x
               - gameplayCamera.WorldToScreenPoint(new Vector3(Values.LaneWidth, 0, 0)).x)
               / 2;

            Values.ScreenSizeBase = gameplayCamera.pixelWidth;
            Values.ScreenSize = gameplayCamera.pixelWidth;
        }

        private void OnDestroy()
        {
            Settings.InputMode.OnValueChanged.RemoveListener(OnInputModeChange);
            InputSystem.pollingFrequency = 60;
        }

        private void OnInputModeChange(int modeNum)
        {
            InputMode inputMode = (InputMode)modeNum;
            inputHandler?.ResetJudge();

            switch (inputMode)
            {
                case InputMode.Auto:
                    inputHandler = new AutoInputHandler();
                    isAuto = true;
                    break;
                case InputMode.AutoController:
                    inputHandler = new AutoControllerInputHandler();
                    isAuto = true;
                    break;
                case InputMode.Controller:
                    inputHandler = new ControllerInputHandler();
                    isAuto = false;
                    break;
                case InputMode.Mouse:
                    inputHandler = new MouseInputHandler();
                    isAuto = false;
                    break;
                case InputMode.Touch:
                    inputHandler = new TouchInputHandler();
                    isAuto = false;
                    break;
                default:
                    inputHandler = new IdleInputHandler();
                    isAuto = false;
                    break;
            }
        }
    }
}