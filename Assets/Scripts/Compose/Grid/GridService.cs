using ArcCreate.Compose.Navigation;
using ArcCreate.Utility.Parser;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Grid
{
    [EditorScope("Grid")]
    public class GridService : MonoBehaviour, IGridService
    {
        [SerializeField] private TimingGrid timingGrid;
        [SerializeField] private VerticalGrid verticalGrid;
        [SerializeField] private TMP_Dropdown gridSlotDropdown;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Toggle useDefaultToggle;
        [SerializeField] private TMP_InputField fromLaneField;
        [SerializeField] private TMP_InputField toLaneField;
        [SerializeField] private Toggle scaleGridToggle;
        [SerializeField] private TMP_InputField scriptField;
        private bool isGridEnabled;

        public bool IsGridEnabled
        {
            get => isGridEnabled;
            set
            {
                isGridEnabled = value;
                timingGrid.SetGridEnabled(value);
                verticalGrid.SetGridEnabled(value);
            }
        }

        private string DefaultGridScript => I18n.S("Compose.Grid.DefaultScript");

        public int MoveTimingBackward(int sourceTiming) => timingGrid.MoveTimingBackward(sourceTiming);

        public int MoveTimingForward(int sourceTiming) => timingGrid.MoveTimingForward(sourceTiming);

        public int MoveTimingBackwardByBeat(int sourceTiming) => timingGrid.MoveTimingBackwardByBeat(sourceTiming);

        public int MoveTimingForwardByBeat(int sourceTiming) => timingGrid.MoveTimingForwardByBeat(sourceTiming);

        public int SnapTimingToGrid(int sourceTiming) => timingGrid.SnapToTimingGrid(sourceTiming);

        public int SnapTimingToGridIfGridIsEnabled(int sourceTiming) => IsGridEnabled ? SnapTimingToGrid(sourceTiming) : sourceTiming;

        public Vector2 SnapPointToGridIfEnabled(Vector2 point) => IsGridEnabled ? SnapPointToGrid(point) : point;

        public Vector2 SnapPointToGrid(Vector2 point) => verticalGrid.SnapToVerticalGrid(point);

        [EditorAction("Toggle", true)]
        [RequireGameplayLoaded]
        public void Toggle()
        {
            IsGridEnabled = !IsGridEnabled;
        }

        [EditorAction("ToggleSlot", false, "g")]
        [SubAction("Confirm", false, "<u-g>")]
        [SubAction("Slot1", false, "1")]
        [SubAction("Slot2", false, "2")]
        [SubAction("Slot3", false, "3")]
        [SubAction("Slot4", false, "4")]
        [SubAction("Slot5", false, "5")]
        [SubAction("Slot6", false, "6")]
        [SubAction("Slot7", false, "7")]
        [SubAction("Slot8", false, "8")]
        [SubAction("Slot9", false, "9")]
        [SubAction("Slot0", false, "0")]
        [RequireGameplayLoaded]
        public async UniTask SetSlot(EditorAction action)
        {
            SubAction stop = action.GetSubAction("Confirm");
            SubAction[] slots = new SubAction[]
            {
                action.GetSubAction("Slot1"),
                action.GetSubAction("Slot2"),
                action.GetSubAction("Slot3"),
                action.GetSubAction("Slot4"),
                action.GetSubAction("Slot5"),
                action.GetSubAction("Slot6"),
                action.GetSubAction("Slot7"),
                action.GetSubAction("Slot8"),
                action.GetSubAction("Slot9"),
                action.GetSubAction("Slot0"),
            };

            bool slotSwitched = false;

            while (!stop.WasExecuted)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    SubAction subAction = slots[i];
                    if (subAction.WasExecuted)
                    {
                        Settings.GridSlot.Value = i;
                        slotSwitched = true;
                    }
                }

                await UniTask.NextFrame();
            }

            IsGridEnabled = slotSwitched || !IsGridEnabled;
        }

        private void LoadGridSlot(int slot)
        {
            GridSettings settings = GridSettings.GetSlot(slot);
            IVerticalGridSettings vertical = settings.GetVerticalSettings(DefaultGridScript);
            verticalGrid.LoadGridSettings(
                vertical.ColliderArea,
                vertical.PanelColor,
                vertical.SnapTolerance,
                vertical.Lines,
                vertical.Areas,
                settings.ScaleGridToSkyInput.Value);
            timingGrid.LoadGridSettings(
                settings.FromLane.Value,
                settings.ToLane.Value);

            useDefaultToggle.SetIsOnWithoutNotify(settings.UseDefaultSettings.Value);
            fromLaneField.SetTextWithoutNotify(settings.FromLane.Value.ToString());
            toLaneField.SetTextWithoutNotify(settings.ToLane.Value.ToString());
            scaleGridToggle.SetIsOnWithoutNotify(settings.ScaleGridToSkyInput.Value);
            scriptField.SetTextWithoutNotify(
                settings.UseDefaultSettings.Value ?
                string.Empty :
                settings.ScriptWithFallback(DefaultGridScript));

            canvasGroup.interactable = !settings.UseDefaultSettings.Value;
        }

        private void Awake()
        {
            IsGridEnabled = false;
            timingGrid.Setup();
            verticalGrid.Setup();
            GridSettings.Initialize();

            useDefaultToggle.onValueChanged.AddListener(OnUseDefaultToggle);
            fromLaneField.onEndEdit.AddListener(OnFromLaneField);
            toLaneField.onEndEdit.AddListener(OnToLaneField);
            scaleGridToggle.onValueChanged.AddListener(OnScaleGridToggle);
            scriptField.onEndEdit.AddListener(OnScriptField);

            gridSlotDropdown.onValueChanged.AddListener(OnGridSlotDropdown);
            Settings.GridSlot.OnValueChanged.AddListener(OnSettingsGridSlot);

            LoadGridSlot(Settings.GridSlot.Value);
        }

        private void OnDestroy()
        {
            useDefaultToggle.onValueChanged.RemoveListener(OnUseDefaultToggle);
            fromLaneField.onEndEdit.RemoveListener(OnFromLaneField);
            toLaneField.onEndEdit.RemoveListener(OnToLaneField);
            scaleGridToggle.onValueChanged.RemoveListener(OnScaleGridToggle);
            scriptField.onEndEdit.RemoveListener(OnScriptField);

            gridSlotDropdown.onValueChanged.RemoveListener(OnGridSlotDropdown);
            Settings.GridSlot.OnValueChanged.RemoveListener(OnSettingsGridSlot);
        }

        private void OnGridSlotDropdown(int val)
        {
            Settings.GridSlot.Value = val;
        }

        private void OnSettingsGridSlot(int val)
        {
            LoadGridSlot(val);
            gridSlotDropdown.SetValueWithoutNotify(val);
        }

        private void OnUseDefaultToggle(bool value)
        {
            GridSettings settings = GridSettings.GetSlot(Settings.GridSlot.Value);
            settings.UseDefaultSettings.Value = value;
            LoadGridSlot(Settings.GridSlot.Value);
        }

        private void OnFromLaneField(string value)
        {
            if (Evaluator.TryInt(value, out int lane))
            {
                GridSettings settings = GridSettings.GetSlot(Settings.GridSlot.Value);
                settings.FromLane.Value = lane;
                LoadGridSlot(Settings.GridSlot.Value);
            }
        }

        private void OnToLaneField(string value)
        {
            if (Evaluator.TryInt(value, out int lane))
            {
                GridSettings settings = GridSettings.GetSlot(Settings.GridSlot.Value);
                settings.ToLane.Value = lane;
                LoadGridSlot(Settings.GridSlot.Value);
            }
        }

        private void OnScaleGridToggle(bool value)
        {
            GridSettings settings = GridSettings.GetSlot(Settings.GridSlot.Value);
            settings.ScaleGridToSkyInput.Value = value;
            LoadGridSlot(Settings.GridSlot.Value);
        }

        private void OnScriptField(string value)
        {
            GridSettings settings = GridSettings.GetSlot(Settings.GridSlot.Value);
            settings.Script.Value = value;
            LoadGridSlot(Settings.GridSlot.Value);
        }
    }
}