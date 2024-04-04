using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
    [EditorScope("Camera")]
    public class CameraTable : Table<CameraEvent>
    {
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private Button addButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button freeCameraButton;
        [SerializeField] private Image freeCameraButtonImage;
        [SerializeField] private Color highlightColor;
        [SerializeField] private Color normalColor;
        [SerializeField] private MarkerRange marker;

        public override CameraEvent Selected
        {
            get => base.Selected;
            set
            {
                base.Selected = value;
                marker.gameObject.SetActive(value != null);
                RequireCameraEventSelectedAttribute.IsSelected = value != null;
                UpdateMarker();
            }
        }

        public void Rebuild()
        {
            ReloadGroup(Values.EditingTimingGroup.Value);
        }

        public void UpdateMarker()
        {
            if (Selected == null)
            {
                return;
            }

            marker.SetTiming(Selected.Timing, Selected.Timing + Selected.Duration);
        }

        [EditorAction("FreeCamera", true)]
        [RequireCameraEventSelected]
        [SubAction("MoveForward", false, "<h-w>")]
        [SubAction("MoveLeft", false, "<h-a>")]
        [SubAction("MoveBackward", false, "<h-s>")]
        [SubAction("MoveRight", false, "<h-d>")]
        [SubAction("MoveUp", false, "<h-space>")]
        [SubAction("MoveDown", false, "<h-shift>")]
        [SubAction("LookUp", false, "<h-i>")]
        [SubAction("LookLeft", false, "<h-j>")]
        [SubAction("LookDown", false, "<h-k>")]
        [SubAction("LookRight", false, "<h-l>")]
        [SubAction("RollLeft", false, "<h-u>")]
        [SubAction("RollRight", false, "<h-o>")]
        [SubAction("Faster", false, "<h-ctrl>")]
        [SubAction("Slower", false, "<h-alt>")]
        [SubAction("Cancel", false, "<esc>")]
        [SubAction("Confirm", false, "<cr>")]
        [KeybindHint("Confirm", Priority = KeybindPriorities.SubConfirm)]
        [KeybindHint("Cancel", Priority = KeybindPriorities.SubCancel)]
        [KeybindHint("Faster", Priority = KeybindPriorities.FreeCamera + 97)]
        [KeybindHint("Slower", Priority = KeybindPriorities.FreeCamera + 96)]
        [KeybindHint("MoveForward", Priority = KeybindPriorities.FreeCamera + 11)]
        [KeybindHint("MoveLeft", Priority = KeybindPriorities.FreeCamera + 10)]
        [KeybindHint("MoveRight", Priority = KeybindPriorities.FreeCamera + 9)]
        [KeybindHint("MoveBackward", Priority = KeybindPriorities.FreeCamera + 8)]
        [KeybindHint("MoveUp", Priority = KeybindPriorities.FreeCamera + 7)]
        [KeybindHint("MoveDown", Priority = KeybindPriorities.FreeCamera + 6)]
        [KeybindHint("LookUp", Priority = KeybindPriorities.FreeCamera + 5)]
        [KeybindHint("LookDown", Priority = KeybindPriorities.FreeCamera + 4)]
        [KeybindHint("LookLeft", Priority = KeybindPriorities.FreeCamera + 3)]
        [KeybindHint("LookRight", Priority = KeybindPriorities.FreeCamera + 2)]
        [KeybindHint("RollLeft", Priority = KeybindPriorities.FreeCamera + 1)]
        [KeybindHint("RollRight", Priority = KeybindPriorities.FreeCamera + 0)]
        public async UniTask BeginFreeCamera(EditorAction action)
        {
            SubAction moveForward = action.GetSubAction("MoveForward");
            SubAction moveLeft = action.GetSubAction("MoveLeft");
            SubAction moveBackward = action.GetSubAction("MoveBackward");
            SubAction moveRight = action.GetSubAction("MoveRight");
            SubAction moveUp = action.GetSubAction("MoveUp");
            SubAction moveDown = action.GetSubAction("MoveDown");
            SubAction lookUp = action.GetSubAction("LookUp");
            SubAction lookLeft = action.GetSubAction("LookLeft");
            SubAction lookDown = action.GetSubAction("LookDown");
            SubAction lookRight = action.GetSubAction("LookRight");
            SubAction rollLeft = action.GetSubAction("RollLeft");
            SubAction rollRight = action.GetSubAction("RollRight");
            SubAction faster = action.GetSubAction("Faster");
            SubAction slower = action.GetSubAction("Slower");
            SubAction cancel = action.GetSubAction("Cancel");
            SubAction confirm = action.GetSubAction("Confirm");

            CameraEvent target = Selected;

            int index = IndexOf(target);
            CameraEvent nextToTarget = (index >= 0 && index < Data.Count - 1) ? Data[index + 1] : null;
            Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.FreeCameraEditHelp"));

            Camera cam = Services.Gameplay.Camera.GameplayCamera;
            CameraEvent oldValue = target.Clone() as CameraEvent;

            Services.Gameplay.Audio.ChartTiming = target.Timing + target.Duration;
            EventSystem.current.SetSelectedGameObject(null);

            List<CameraEvent> updateList = new List<CameraEvent> { target };
            Quaternion rotateRightBy90 = Quaternion.Euler(0, 90, 0);
            float sensitivity = Settings.CameraSensitivity.Value;

            freeCameraButtonImage.color = highlightColor;

            while (true)
            {
                float rotAmount = sensitivity * Time.deltaTime;
                float movAmount = rotAmount * 50;

                if (faster.WasExecuted)
                {
                    rotAmount *= 4;
                    movAmount *= 4;
                }

                if (slower.WasExecuted)
                {
                    rotAmount /= 4;
                    movAmount /= 4;
                }

                Vector3 forwardVector = cam.transform.forward.normalized;
                forwardVector.x = -forwardVector.x;
                forwardVector.y = 0;
                Vector3 leftVector = new Vector3(forwardVector.z, 0, -forwardVector.x);

                if (moveForward.WasExecuted)
                {
                    target.Move = target.Move + (forwardVector * movAmount);
                }

                if (moveBackward.WasExecuted)
                {
                    target.Move = target.Move - (forwardVector * movAmount);
                }

                if (moveLeft.WasExecuted)
                {
                    target.Move = target.Move + (leftVector * movAmount);
                }

                if (moveRight.WasExecuted)
                {
                    target.Move = target.Move - (leftVector * movAmount);
                }

                if (moveUp.WasExecuted)
                {
                    target.Move = new Vector3(target.Move.x, target.Move.y + movAmount, target.Move.z);
                }

                if (moveDown.WasExecuted)
                {
                    target.Move = new Vector3(target.Move.x, target.Move.y - movAmount, target.Move.z);
                }

                if (lookUp.WasExecuted)
                {
                    target.Rotate = new Vector3(target.Rotate.x, target.Rotate.y + rotAmount, target.Rotate.z);
                }

                if (lookDown.WasExecuted)
                {
                    target.Rotate = new Vector3(target.Rotate.x, target.Rotate.y - rotAmount, target.Rotate.z);
                }

                if (lookLeft.WasExecuted)
                {
                    target.Rotate = new Vector3(target.Rotate.x + rotAmount, target.Rotate.y, target.Rotate.z);
                }

                if (lookRight.WasExecuted)
                {
                    target.Rotate = new Vector3(target.Rotate.x - rotAmount, target.Rotate.y, target.Rotate.z);
                }

                if (rollLeft.WasExecuted)
                {
                    target.Rotate = new Vector3(target.Rotate.x, target.Rotate.y, target.Rotate.z - rotAmount);
                }

                if (rollRight.WasExecuted)
                {
                    target.Rotate = new Vector3(target.Rotate.x, target.Rotate.y, target.Rotate.z + rotAmount);
                }

                target.Move = new Vector3(
                    Mathf.Round(target.Move.x * 100) / 100,
                    Mathf.Round(target.Move.y * 100) / 100,
                    Mathf.Round(target.Move.z * 100) / 100);
                target.Rotate = new Vector3(
                    Mathf.Round(target.Rotate.x * 100) / 100,
                    Mathf.Round(target.Rotate.y * 100) / 100,
                    Mathf.Round(target.Rotate.z * 100) / 100);

                Services.Gameplay.Chart.UpdateEvents(updateList);

                if (confirm.WasExecuted)
                {
                    Rebuild();

                    CameraEvent newValue = target.Clone() as CameraEvent;
                    target.Assign(oldValue);

                    if (nextToTarget != null)
                    {
                        CameraEvent newValueForNext = nextToTarget.Clone() as CameraEvent;
                        newValueForNext.Move -= newValue.Move - oldValue.Move;
                        newValueForNext.Rotate -= newValue.Rotate - oldValue.Rotate;
                        Services.History.AddCommand(new EventCommand(
                            name: I18n.S("Compose.Notify.History.EditCamera"),
                            update: new List<(ArcEvent instance, ArcEvent newValue)> { (target, newValue), (nextToTarget, newValueForNext) }));
                    }
                    else
                    {
                        Services.History.AddCommand(new EventCommand(
                            name: I18n.S("Compose.Notify.History.EditCamera"),
                            update: new List<(ArcEvent instance, ArcEvent newValue)> { (target, newValue) }));
                    }

                    freeCameraButtonImage.color = normalColor;
                    return;
                }

                if (cancel.WasExecuted)
                {
                    target.Assign(oldValue);
                    Services.Gameplay.Chart.UpdateEvents(updateList);
                    freeCameraButtonImage.color = normalColor;
                    return;
                }

                await UniTask.NextFrame();
            }
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void Awake()
        {
            base.Awake();
            Values.EditingTimingGroup.OnValueChange += OnEdittingTimingGroup;
            gameplayData.OnChartFileLoad += OnChart;
            gameplayData.OnChartCameraEdit += OnChartEdit;
            addButton.onClick.AddListener(OnAddButton);
            removeButton.onClick.AddListener(OnRemoveButton);
            freeCameraButton.onClick.AddListener(OnFreeCameraButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Values.EditingTimingGroup.OnValueChange -= OnEdittingTimingGroup;
            gameplayData.OnChartFileLoad -= OnChart;
            gameplayData.OnChartCameraEdit -= OnChartEdit;
            addButton.onClick.RemoveListener(OnAddButton);
            removeButton.onClick.RemoveListener(OnRemoveButton);
            freeCameraButton.onClick.RemoveListener(OnFreeCameraButton);
        }

        private void OnChartEdit()
        {
            Rebuild();
            UpdateMarker();
        }

        private void OnAddButton()
        {
            CameraEvent cam;

            if (Selected == null)
            {
                int t = 0;
                if (Data.Count > 0)
                {
                    t = Data[Data.Count - 1].Timing + 1;
                }

                cam = new CameraEvent()
                {
                    Timing = t,
                    Move = Vector3.zero,
                    Rotate = Vector3.zero,
                    CameraType = Gameplay.Data.CameraType.L,
                    Duration = 1,
                    TimingGroup = Values.EditingTimingGroup.Value,
                };
            }
            else
            {
                int t = Selected.Timing + 1;

                while (true)
                {
                    foreach (CameraEvent ev in Data)
                    {
                        if (ev.Timing == t)
                        {
                            t += 1;
                            continue;
                        }
                    }

                    cam = new CameraEvent()
                    {
                        Timing = t,
                        Move = Vector3.zero,
                        Rotate = Vector3.zero,
                        CameraType = Gameplay.Data.CameraType.L,
                        Duration = 1,
                        TimingGroup = Values.EditingTimingGroup.Value,
                    };
                    break;
                }
            }

            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.AddCamera"),
                add: new List<ArcEvent>() { cam }));
            Selected = cam;
            Rebuild();
            JumpTo(IndexOf(cam));
        }

        private void OnRemoveButton()
        {
            if (Selected == null)
            {
                return;
            }

            int index = IndexOf(Selected);
            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.RemoveCamera"),
                remove: new List<ArcEvent>() { Selected }));
            Selected = Data.Count == 0 ? null : Data[Mathf.Max(index - 1, 0)];
            Rebuild();
            JumpTo(index - 1);
        }

        private void OnEdittingTimingGroup(int group)
        {
            Selected = null;
            ReloadGroup(group);
        }

        private void OnChart()
        {
            Selected = null;
            ReloadGroup(0);
        }

        private void ReloadGroup(int group)
        {
            List<CameraEvent> cam = Services.Gameplay.Chart.GetAll<CameraEvent>().Where(c => c.TimingGroup == group).ToList();
            SetData(cam);
        }

        private void OnEnable()
        {
            ReloadGroup(Values.EditingTimingGroup.Value);
            if (marker != null)
            {
                marker.gameObject.SetActive(Selected != null);
            }
        }

        private void OnDisable()
        {
            if (marker != null)
            {
                marker.gameObject.SetActive(false);
            }
        }

        private void OnFreeCameraButton()
        {
            if (Selected == null)
            {
                return;
            }

            JumpTo(IndexOf(Selected));
            Services.Navigation.StartAction("Camera.FreeCamera");
        }

        public class RequireCameraEventSelectedAttribute : ContextRequirementAttribute
        {
            public static bool IsSelected { get; set; } = false;

            public override bool CheckRequirement()
            {
                return IsSelected;
            }
        }
    }
}