using System;
using ArcCreate.Compose.Components;
using ArcCreate.Compose.Navigation;
using ArcCreate.Utility.Extension;
using ArcCreate.Utility.Parser;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Compose.Cursor
{
    [EditorScope("Cursor")]
    public class CursorService : MonoBehaviour, ICursorService
    {
        [SerializeField] private MeshCollider laneCollider;
        [SerializeField] private MeshCollider verticalCollider;
        [SerializeField] private LineRenderer cursorLaneTiming;
        [SerializeField] private LineRenderer cursorLaneX;
        [SerializeField] private LineRenderer cursorVerticalX;
        [SerializeField] private LineRenderer cursorVerticalY;
        [SerializeField] private Camera editorCamera;
        [SerializeField] private RectTransform gameplayViewport;
        private bool isCursorAboveViewport;
        private bool isLaneCursorEnabled;
        private bool isHittingLane;
        private bool isHittingVertical;
        private int selectingTiming;
        private int selectingLane;
        private Vector2 selectingVerticalPoint;
        private Vector3 cursorWorldPosition;
        private int showVerticalAtTiming;
        private Action onRemoveDigit;
        private Action onTypedValueConfirm;
        private Action<string> onClipboard;

        public bool EnableLaneCursor
        {
            get => isLaneCursorEnabled;
            set => isLaneCursorEnabled = value;
        }

        public int CursorTiming => selectingTiming;

        public int CursorLane => selectingLane;

        public bool IsHittingLane => isHittingLane;

        public bool IsCursorAboveViewport => isCursorAboveViewport;

        public Vector3 CursorWorldPosition => cursorWorldPosition;

        public async UniTask<(bool wasSuccessful, int timing)> RequestTimingSelection(
            SubAction confirm,
            SubAction cancel,
            Action<int> update = null,
            Func<int, bool> constraint = null)
        {
            isLaneCursorEnabled = true;
            var result = await RequestValue(
                confirm: confirm,
                cancel: cancel,
                selector: () => selectingTiming,
                isValidTypedChar: char.IsDigit,
                convertTypedStringToValue: (s) =>
                {
                    bool valid = Evaluator.TryInt(s, out int val);
                    return (valid, val);
                },
                update: update,
                constraint: constraint);

            isLaneCursorEnabled = false;
            return result;
        }

        public async UniTask<(bool wasSuccessful, int lane)> RequestLaneSelection(
            SubAction confirm,
            SubAction cancel,
            Action<int> update = null,
            Func<int, bool> constraint = null)
        {
            isLaneCursorEnabled = true;
            var result = await RequestValue(
                confirm: confirm,
                cancel: cancel,
                selector: () => selectingLane,
                isValidTypedChar: char.IsDigit,
                convertTypedStringToValue: (s) =>
                {
                    bool valid = Evaluator.TryInt(s, out int val);
                    return (valid, val);
                },
                update: update,
                constraint: (lane) => (constraint?.Invoke(lane) ?? true) && lane >= Gameplay.Values.LaneFrom && lane <= Gameplay.Values.LaneTo);

            isLaneCursorEnabled = false;
            return result;
        }

        public async UniTask<(bool wasSuccessful, Vector2 point)> RequestVerticalSelection(
            SubAction confirm,
            SubAction cancel,
            int showVerticalAtTiming,
            Action<Vector2> update = null,
            Func<Vector2, bool> constraint = null)
        {
            this.showVerticalAtTiming = showVerticalAtTiming;
            verticalCollider.gameObject.SetActive(true);
            var result = await RequestValue(
                confirm: confirm,
                cancel: cancel,
                selector: () => selectingVerticalPoint,
                isValidTypedChar: (c) => char.IsDigit(c) || c == ',' || c == '.',
                convertTypedStringToValue: (s) =>
                {
                    string[] split = s.Split(',');
                    if (split.Length != 2)
                    {
                        return (false, default);
                    }

                    float y = default;
                    bool valid = Evaluator.TryFloat(split[0], out float x) && Evaluator.TryFloat(split[1], out y);
                    return (valid, new Vector2(x, y));
                },
                update: update,
                constraint: constraint);

            verticalCollider.gameObject.SetActive(false);
            return result;
        }

        [EditorAction("RemoveDigit", false, "<backspace>")]
        [RequireTyping]
        public void TypedValueRemoveDigit()
        {
            onRemoveDigit?.Invoke();
        }

        [EditorAction("ConfirmTypedValue", false, "<cr>")]
        [RequireTyping]
        public void ConfirmTypedValue()
        {
            onTypedValueConfirm?.Invoke();
        }

        [EditorAction("PasteToTypedValue", false, "<c-v>")]
        [RequireTyping]
        public void PasteToTypedValue()
        {
            onClipboard?.Invoke(GUIUtility.systemCopyBuffer);
        }

#if UNITY_EDITOR
        [EditorAction("Test", false, "tv")]
        [SubAction("Confirm", false, "<cr>", "<u-mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [WhitelistScopes(typeof(CursorService), typeof(Grid.GridService))]
        public async UniTask TestCursor(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");
            (bool timingSuccess, int timing) = await RequestTimingSelection(confirm, cancel);
            if (!timingSuccess)
            {
                return;
            }

            (bool pointSuccess, Vector2 point) = await RequestVerticalSelection(confirm, cancel, timing);
            if (pointSuccess)
            {
                print(point);
            }
        }
#endif

        public void ForceUpdateLaneCursor()
        {
            Camera gameplayCamera = Services.Gameplay.Camera.GameplayCamera;
            Vector2 mousePosition = Input.mousePosition;
            Ray ray = gameplayCamera.ScreenPointToRay(mousePosition);
            bool isCursorHoveringOnTrack = laneCollider.Raycast(ray, out RaycastHit laneHit, 120);
            isHittingLane = isLaneCursorEnabled && isCursorHoveringOnTrack;
            if (isCursorHoveringOnTrack)
            {
                AlignLaneCursor(laneHit);
            }
        }

        private void Update()
        {
            if (Services.Gameplay?.IsLoaded ?? false)
            {
                Camera gameplayCamera = Services.Gameplay.Camera.GameplayCamera;
                Vector2 mousePosition = Input.mousePosition;
                isCursorAboveViewport = !Dialog.IsAnyOpen && RectTransformUtility.RectangleContainsScreenPoint(gameplayViewport, mousePosition, editorCamera);
                if (!isCursorAboveViewport)
                {
                    return;
                }

                Ray ray = gameplayCamera.ScreenPointToRay(mousePosition);

                bool isVerticalActive = verticalCollider.gameObject.activeInHierarchy;
                bool isCursorHoveringOnTrack = laneCollider.Raycast(ray, out RaycastHit laneHit, 120);
                isHittingVertical = verticalCollider.Raycast(ray, out RaycastHit verticalHit, 120);
                isHittingLane = isLaneCursorEnabled && isCursorHoveringOnTrack;

                if (isCursorHoveringOnTrack)
                {
                    CheckScroll();
                }

                if (!isHittingVertical && isHittingLane)
                {
                    AlignLaneCursor(laneHit);
                }
                else
                {
                    DisableLaneCursor();
                }

                if (isHittingVertical)
                {
                    AlignVerticalCursor(verticalHit);
                }
                else
                {
                    DisableVerticalCursor();
                }

                if (verticalCollider.gameObject.activeInHierarchy)
                {
                    UpdateVerticalZPosition();
                }
            }
        }

        private void CheckScroll()
        {
            float scrollY = Input.mouseScrollDelta.y;
            if (scrollY == 0 || Mathf.Abs(scrollY) < Settings.TrackScrollThreshold.Value)
            {
                return;
            }

            scrollY *= Settings.ScrollSensitivityVertical.Value;

            int timing = Services.Gameplay.Audio.ChartTiming;
            int snap = scrollY > 0 ? Services.Grid.MoveTimingForward(timing) : Services.Grid.MoveTimingBackward(timing);
            snap = Mathf.Clamp(snap, timing - Settings.TrackScrollMaxMovement.Value, timing + Settings.TrackScrollMaxMovement.Value);

            Services.Gameplay.Audio.ChartTiming = snap;
        }

        private void AlignLaneCursor(RaycastHit hit)
        {
            cursorLaneTiming.gameObject.SetActive(true);
            cursorLaneX.gameObject.SetActive(true);

            var tg = Services.Gameplay.Chart.GetTimingGroup(Values.EditingTimingGroup.Value);
            int timing = tg.GetTimingFromZPosition(hit.point.z);
            selectingTiming = Services.Grid.SnapTimingToGridIfGridIsEnabled(timing);
            double fp = tg.GetFloorPositionFromCurrent(selectingTiming);
            float z = Gameplay.ArcFormula.FloorPositionToZ(fp);

            cursorLaneTiming.DrawLine(
                from: new Vector3(Values.LaneFromX, 0, z),
                to: new Vector3(Values.LaneToX, 0, z));

            cursorLaneX.DrawLine(
                from: new Vector3(hit.point.x, 0, Gameplay.Values.TrackLengthBackward),
                to: new Vector3(hit.point.x, 0, -Gameplay.Values.TrackLengthForward));

            selectingLane = Gameplay.ArcFormula.WorldXToLane(hit.point.x);
            cursorWorldPosition.z = z;
            cursorWorldPosition.x = hit.point.x;
        }

        private void DisableLaneCursor()
        {
            cursorLaneTiming.gameObject.SetActive(false);
            cursorLaneX.gameObject.SetActive(false);
        }

        private void AlignVerticalCursor(RaycastHit hit)
        {
            cursorVerticalX.gameObject.SetActive(true);
            cursorVerticalY.gameObject.SetActive(true);

            // TODO: Extend for 6k
            float minLane = -8.5f;
            float maxLane = 8.5f;

            Vector2 snapped = Services.Grid.SnapPointToGridIfEnabled(hit.point);
            float verticalScale = verticalCollider.transform.localScale.y;

            selectingVerticalPoint = new Vector2(
                Gameplay.ArcFormula.WorldXToArc(snapped.x),
                Gameplay.ArcFormula.WorldYToArc(snapped.y));

            if (verticalScale > Mathf.Epsilon)
            {
                snapped.y /= verticalScale;
            }

            cursorVerticalX.DrawLine(
                from: new Vector3(snapped.x, 0, 0),
                to: new Vector3(snapped.x, Gameplay.Values.ArcY1, 0));

            cursorVerticalY.DrawLine(
                from: new Vector3(minLane, snapped.y, 0),
                to: new Vector3(maxLane, snapped.y, 0));
            cursorWorldPosition.x = snapped.x;

            cursorWorldPosition.y = snapped.y;
            cursorWorldPosition.z = hit.point.z;
        }

        private void DisableVerticalCursor()
        {
            cursorVerticalX.gameObject.SetActive(false);
            cursorVerticalY.gameObject.SetActive(false);
        }

        private void UpdateVerticalZPosition()
        {
            var tg = Services.Gameplay.Chart.GetTimingGroup(Values.EditingTimingGroup.Value);
            double fp = tg.GetFloorPositionFromCurrent(showVerticalAtTiming);
            float z = Gameplay.ArcFormula.FloorPositionToZ(fp);

            Vector3 pos = verticalCollider.transform.localPosition;
            pos.z = z;
            verticalCollider.transform.localPosition = pos;
        }

        // Amazing
        private async UniTask<(bool wasSuccessful, T value)> RequestValue<T>(
            SubAction confirm,
            SubAction cancel,
            Func<T> selector,
            Func<char, bool> isValidTypedChar,
            Func<string, (bool, T)> convertTypedStringToValue,
            Action<T> update = null,
            Func<T, bool> constraint = null)
        {
            T result = default;
            bool resultSet = false;

            // Printing the value every time anyway, StringBuilder doesn't help
            string typedValue = "";
            bool typedValueConfirmed = false;
            RequireTypingAttribute.IsTyping = true;

            void RemoveDigit()
            {
                typedValue = typedValue.Remove(typedValue.Length - 1, 1);
                Services.Popups.Notify(Popups.Severity.Info, typedValue);
            }

            void ConfirmTypedValue()
            {
                typedValueConfirmed = true;
            }

            void PasteClipboard(string clipboard)
            {
                (bool valid, T value) = convertTypedStringToValue.Invoke(clipboard);
                if (valid)
                {
                    typedValue = clipboard;
                    Services.Popups.Notify(Popups.Severity.Info, typedValue);
                }
            }

            // Keyboard.current.onTextInput += OnType;
            onRemoveDigit += RemoveDigit;
            onTypedValueConfirm += ConfirmTypedValue;
            onClipboard += PasteClipboard;

            bool wasSuccessful = false;
            while (true)
            {
                T selecting = selector.Invoke();
                bool valid = constraint?.Invoke(selecting) ?? true;
                if (valid)
                {
                    result = selecting;
                    resultSet = true;
                    update?.Invoke(result);
                }

                if (typedValueConfirmed)
                {
                    string str = typedValue;
                    (bool validString, T value) = convertTypedStringToValue.Invoke(str);

                    if (validString && (constraint?.Invoke(value) ?? true))
                    {
                        wasSuccessful = true;
                        result = value;
                        resultSet = true;
                        break;
                    }
                    else
                    {
                        Services.Popups.Notify(Popups.Severity.Error, typedValue);
                        typedValueConfirmed = false;
                    }
                }

                if (confirm.WasExecuted)
                {
                    wasSuccessful = resultSet;
                    break;
                }

                if (cancel.WasExecuted)
                {
                    wasSuccessful = false;
                    break;
                }

                if (Input.anyKeyDown)
                {
                    foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKeyDown(k)
                        && ConvertKeyCodeToChar(k, out char c))
                        {
                            if (isValidTypedChar.Invoke(c))
                            {
                                typedValue += c;
                                Services.Popups.Notify(Popups.Severity.Info, typedValue);
                            }
                        }
                    }
                }

                await UniTask.NextFrame();
            }

            RequireTypingAttribute.IsTyping = false;
            onRemoveDigit -= RemoveDigit;
            onTypedValueConfirm -= ConfirmTypedValue;
            onClipboard -= PasteClipboard;
            return (wasSuccessful, result);
        }

        private bool ConvertKeyCodeToChar(KeyCode k, out char c)
        {
            c = default;
            string str = k.ToString().ToLower();

            switch (k)
            {
                case KeyCode.Comma:
                    c = ',';
                    return true;
                case KeyCode.Period:
                    c = '.';
                    return true;
            }

            if (str.StartsWith("alpha"))
            {
                str = str.Substring("alpha".Length);
            }

            if (str.StartsWith("keypad"))
            {
                str = str.Substring("keypad".Length);
            }

            if (str.Length == 1)
            {
                c = str[0];
                return true;
            }

            return false;
        }

        private class RequireTypingAttribute : ContextRequirementAttribute
        {
            public static bool IsTyping { get; set; } = false;

            public override bool CheckRequirement() => IsTyping;
        }
    }
}