using System.Collections.Generic;
using System.Linq;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArcCreate.Compose.EventsEditor
{
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

        protected override void Update()
        {
            base.Update();
        }

        protected override void Awake()
        {
            base.Awake();
            Values.EditingTimingGroup.OnValueChange += OnEdittingTimingGroup;
            gameplayData.OnChartFileLoad += OnChart;
            gameplayData.OnChartEdit += OnChartEdit;
            addButton.onClick.AddListener(OnAddButton);
            removeButton.onClick.AddListener(OnRemoveButton);
            freeCameraButton.onClick.AddListener(OnFreeCameraButton);
            marker.OnDragDebounced += OnMarker;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Values.EditingTimingGroup.OnValueChange -= OnEdittingTimingGroup;
            gameplayData.OnChartFileLoad -= OnChart;
            gameplayData.OnChartEdit -= OnChartEdit;
            addButton.onClick.RemoveListener(OnAddButton);
            removeButton.onClick.RemoveListener(OnRemoveButton);
            freeCameraButton.onClick.RemoveListener(OnFreeCameraButton);
            marker.OnDragDebounced -= OnMarker;
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
            Selected = Data[Mathf.Max(index - 1, 0)];
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

        private void OnMarker(int timing, int endTiming)
        {
            if (Selected == null)
            {
                return;
            }

            CameraEvent newValue = Selected.Clone() as CameraEvent;
            newValue.Timing = timing;
            newValue.Duration = endTiming - timing;
            Services.History.AddCommand(new EventCommand(
                name: I18n.S("Compose.Notify.History.EditCamera"),
                update: new List<(ArcEvent instance, ArcEvent newValue)> { (Selected, newValue) }));
            Rebuild();
            JumpTo(IndexOf(Selected));
        }

        private void OnEnable()
        {
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
            BeginFreeCamera(Selected).Forget();
        }

        private async UniTask BeginFreeCamera(CameraEvent target)
        {
            Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.FreeCameraEditHelp"));

            Keyboard keyboard = InputSystem.GetDevice<Keyboard>();
            Camera cam = Services.Gameplay.Camera.GameplayCamera;
            CameraEvent oldValue = target.Clone() as CameraEvent;

            Services.Gameplay.Audio.ChartTiming = target.Timing + target.Duration;
            EventSystem.current.SetSelectedGameObject(null);

            List<CameraEvent> updateList = new List<CameraEvent> { target };
            Quaternion rotateRightBy90 = Quaternion.Euler(0, 90, 0);
            float sensitivity = Settings.CameraSensitivity.Value;

            freeCameraButtonImage.color = highlightColor;

            // TODO: Temporary until a proper hotkey system is implemented
            while (true)
            {
                float rotAmount = sensitivity * Time.deltaTime;
                float movAmount = rotAmount * 50;

                if (keyboard.ctrlKey.isPressed)
                {
                    rotAmount *= 3;
                    movAmount *= 3;
                }

                Vector3 forwardVector = cam.transform.forward.normalized;
                forwardVector.x = -forwardVector.x;
                forwardVector.y = 0;
                Vector3 leftVector = new Vector3(forwardVector.z, 0, -forwardVector.x);

                if (keyboard.wKey.isPressed)
                {
                    // Move forward
                    target.Move = target.Move + (forwardVector * movAmount);
                }

                if (keyboard.sKey.isPressed)
                {
                    // Move backward
                    target.Move = target.Move - (forwardVector * movAmount);
                }

                if (keyboard.aKey.isPressed)
                {
                    // Move left
                    target.Move = target.Move + (leftVector * movAmount);
                }

                if (keyboard.dKey.isPressed)
                {
                    // Move right
                    target.Move = target.Move - (leftVector * movAmount);
                }

                if (keyboard.spaceKey.isPressed)
                {
                    // Move up
                    target.Move = new Vector3(target.Move.x, target.Move.y + movAmount, target.Move.z);
                }

                if (keyboard.shiftKey.isPressed)
                {
                    // Move down
                    target.Move = new Vector3(target.Move.x, target.Move.y - movAmount, target.Move.z);
                }

                if (keyboard.iKey.isPressed)
                {
                    // Rotate up
                    target.Rotate = new Vector3(target.Rotate.x, target.Rotate.y + rotAmount, target.Rotate.z);
                }

                if (keyboard.kKey.isPressed)
                {
                    // Rotate down
                    target.Rotate = new Vector3(target.Rotate.x, target.Rotate.y - rotAmount, target.Rotate.z);
                }

                if (keyboard.jKey.isPressed)
                {
                    // Rotate left
                    target.Rotate = new Vector3(target.Rotate.x + rotAmount, target.Rotate.y, target.Rotate.z);
                }

                if (keyboard.lKey.isPressed)
                {
                    // Rotate right
                    target.Rotate = new Vector3(target.Rotate.x - rotAmount, target.Rotate.y, target.Rotate.z);
                }

                if (keyboard.uKey.isPressed)
                {
                    // Roll left
                    target.Rotate = new Vector3(target.Rotate.x, target.Rotate.y, target.Rotate.z - rotAmount);
                }

                if (keyboard.oKey.isPressed)
                {
                    // Roll right
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

                if (keyboard.enterKey.isPressed)
                {
                    Rebuild();

                    CameraEvent newValue = target.Clone() as CameraEvent;
                    target.Assign(oldValue);
                    Services.History.AddCommand(new EventCommand(
                        name: I18n.S("Compose.Notify.History.EditCamera"),
                        update: new List<(ArcEvent instance, ArcEvent newValue)> { (target, newValue) }));

                    freeCameraButtonImage.color = normalColor;
                    return;
                }

                if (keyboard.escapeKey.isPressed)
                {
                    target.Assign(oldValue);
                    Services.Gameplay.Chart.UpdateEvents(updateList);
                    freeCameraButtonImage.color = normalColor;
                    return;
                }

                await UniTask.NextFrame();
            }
        }
    }
}