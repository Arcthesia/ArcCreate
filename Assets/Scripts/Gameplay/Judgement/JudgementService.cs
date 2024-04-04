using ArcCreate.Gameplay.Judgement.Input;
using ArcCreate.Utility;
using UnityEngine;

namespace ArcCreate.Gameplay.Judgement
{
    public class JudgementService : MonoBehaviour, IJudgementService
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Transform skyInput;
        [SerializeField] private JudgementDebug debug;
        private readonly UnorderedList<LaneTapJudgementRequest> laneTapRequests = new UnorderedList<LaneTapJudgementRequest>(32);
        private readonly UnorderedList<LaneHoldJudgementRequest> laneHoldRequests = new UnorderedList<LaneHoldJudgementRequest>(32);
        private readonly UnorderedList<ArcJudgementRequest> arcRequests = new UnorderedList<ArcJudgementRequest>(32);
        private readonly UnorderedList<ArcTapJudgementRequest> arcTapRequests = new UnorderedList<ArcTapJudgementRequest>(32);
        private readonly UnorderedList<LaneTapJudgementRequest> autoLaneTapRequests = new UnorderedList<LaneTapJudgementRequest>(32);
        private readonly UnorderedList<LaneHoldJudgementRequest> autoLaneHoldRequests = new UnorderedList<LaneHoldJudgementRequest>(32);
        private readonly UnorderedList<ArcJudgementRequest> autoArcRequests = new UnorderedList<ArcJudgementRequest>(32);
        private readonly UnorderedList<ArcTapJudgementRequest> autoArcTapRequests = new UnorderedList<ArcTapJudgementRequest>(32);
        private IInputHandler inputHandler;
        private readonly IInputHandler autoHandler = new AutoInputHandler();
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
            var list = request.Properties.Autoplay ? autoLaneTapRequests : laneTapRequests;
            list.Add(request);
        }

        public void Request(LaneHoldJudgementRequest request)
        {
            var list = request.Properties.Autoplay ? autoLaneHoldRequests : laneHoldRequests;
            list.Add(request);
        }

        public void Request(ArcJudgementRequest request)
        {
            var list = request.Properties.Autoplay ? autoArcRequests : arcRequests;
            list.Add(request);
        }

        public void Request(ArcTapJudgementRequest request)
        {
            var list = request.Properties.Autoplay ? autoArcTapRequests : arcTapRequests;
            list.Add(request);
        }

        public void ResetJudge()
        {
            laneTapRequests.Clear();
            laneHoldRequests.Clear();
            arcRequests.Clear();
            arcTapRequests.Clear();
            autoLaneTapRequests.Clear();
            autoLaneHoldRequests.Clear();
            autoArcRequests.Clear();
            autoArcTapRequests.Clear();
            inputHandler?.ResetJudge();
            autoHandler.ResetJudge();
        }

        public void ProcessInput(int currentTiming)
        {
            if (!Services.Audio.IsPlayingAndNotStationary)
            {
                return;
            }

            bool forceAuto = gameplayData.EnableAutoplayMode.Value;
            if (!isAuto && !forceAuto)
            {
                PruneExpiredRequests(currentTiming);
            }

            IInputHandler handler = forceAuto ? autoHandler : inputHandler;
            handler.PollInput();
            handler.HandleTapRequests(currentTiming, laneTapRequests, arcTapRequests);
            handler.HandleLaneHoldRequests(currentTiming, laneHoldRequests);
            handler.HandleArcRequests(currentTiming, arcRequests);
            autoHandler.HandleTapRequests(currentTiming, autoLaneTapRequests, autoArcTapRequests);
            autoHandler.HandleLaneHoldRequests(currentTiming, autoLaneHoldRequests);
            autoHandler.HandleArcRequests(currentTiming, autoArcRequests);
        }

        public void RefreshInputHandler()
        {
            OnInputModeChange(Settings.InputMode.Value);
        }

        private void PruneExpiredRequests(int currentTiming)
        {
            for (int i = laneTapRequests.Count - 1; i >= 0; i--)
            {
                var req = laneTapRequests[i];
                if (currentTiming >= req.ExpireAtTiming)
                {
                    req.Receiver.ProcessLaneTapJudgement(currentTiming - req.AutoAtTiming, req.Properties);
                    laneTapRequests.RemoveAt(i);
                }
            }

            for (int i = laneHoldRequests.Count - 1; i >= 0; i--)
            {
                var req = laneHoldRequests[i];
                if (currentTiming >= req.ExpireAtTiming)
                {
                    req.Receiver.ProcessLaneHoldJudgement(true, req.IsJudgement, req.Properties);
                    laneHoldRequests.RemoveAt(i);
                }
            }

            for (int i = arcRequests.Count - 1; i >= 0; i--)
            {
                var req = arcRequests[i];
                if (currentTiming >= req.ExpireAtTiming)
                {
                    req.Receiver.ProcessArcJudgement(true, req.IsJudgement, req.Properties);
                    arcRequests.RemoveAt(i);
                }
            }

            for (int i = arcTapRequests.Count - 1; i >= 0; i--)
            {
                var req = arcTapRequests[i];
                if (currentTiming >= req.ExpireAtTiming)
                {
                    req.Receiver.ProcessArcTapJudgement(currentTiming - req.AutoAtTiming, req.Properties);
                    arcTapRequests.RemoveAt(i);
                }
            }
        }

        private void Awake()
        {
            Settings.InputMode.OnValueChanged.AddListener(OnInputModeChange);
            OnInputModeChange(Settings.InputMode.Value);

            float laneScreenHitboxBaseX =
                (gameplayCamera.WorldToScreenPoint(Vector3.zero).x
               - gameplayCamera.WorldToScreenPoint(new Vector3(Values.LaneWidth, 0, 0)).x)
               / 2;

            float laneScreenHitboxBaseY =
                gameplayCamera.WorldToScreenPoint(new Vector3(0, Values.ArcY1 / 4f, 0)).y
               - gameplayCamera.WorldToScreenPoint(Vector3.zero).y;

            Values.LaneScreenHitboxBase = new Vector2(laneScreenHitboxBaseX, laneScreenHitboxBaseY);
            Values.ScreenSizeBase = new Vector2(gameplayCamera.pixelWidth, gameplayCamera.pixelHeight);
            Values.ScreenSize = new Vector2(gameplayCamera.pixelWidth, gameplayCamera.pixelHeight);
        }

        private void OnDestroy()
        {
            Settings.InputMode.OnValueChanged.RemoveListener(OnInputModeChange);
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