using System.Collections.Generic;
using ArcCreate.Compose.Cursor;
using ArcCreate.Compose.Grid;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Editing
{
    [EditorScope("Measure")]
    public class Measurer : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private RectTransform parentRect;
        [SerializeField] private Vector2 delta;
        [SerializeField] private Button timingButton;
        [SerializeField] private Button positionButton;
        [SerializeField] private Button xButton;
        [SerializeField] private Button yButton;

        [SerializeField] private TMP_Text timingText;
        [SerializeField] private TMP_Text positionText;
        [SerializeField] private TMP_Text xText;
        [SerializeField] private TMP_Text yText;

        private readonly List<Arc> selectedArcs = new List<Arc>();
        private int currentTiming;
        private float currentX;
        private float currentY;
        private bool actionButtonPressed;

        [EditorAction("Start", true, "e")]
        [RequireGameplayLoaded]
        [SubAction("CopyTiming", false, "t")]
        [SubAction("CopyPosition", false, "c")]
        [SubAction("CopyX", false, "x")]
        [SubAction("CopyY", false, "y")]
        [SubAction("Confirm", false, "<mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [SubAction("Restart", false, "e")]
        [SubAction("Close", false, "<mouse1>", "<esc>")]
        [WhitelistScopes(typeof(CursorService), typeof(GridService), typeof(TimelineService))]
        public async UniTask OpenMeasurer(EditorAction editorAction)
        {
            SubAction copyTiming = editorAction.GetSubAction("CopyTiming");
            SubAction copyPosition = editorAction.GetSubAction("CopyPosition");
            SubAction copyX = editorAction.GetSubAction("CopyX");
            SubAction copyY = editorAction.GetSubAction("CopyY");
            SubAction confirm = editorAction.GetSubAction("Confirm");
            SubAction cancel = editorAction.GetSubAction("Cancel");
            SubAction restart = editorAction.GetSubAction("Restart");
            SubAction close = editorAction.GetSubAction("Close");

            bool running = true;
            while (running)
            {
                running = false;
                rect.gameObject.SetActive(true);

                HashSet<Note> selectedNotes = Services.Selection.SelectedNotes;
                selectedArcs.Clear();

                foreach (var note in selectedNotes)
                {
                    if (note is Arc a)
                    {
                        selectedArcs.Add(a);
                    }
                }

                _ = confirm.WasExecuted;
                _ = cancel.WasExecuted;
                await Services.Cursor.RequestTimingSelection(confirm, cancel, UpdateWindow);
                await UniTask.NextFrame();
                _ = close.WasExecuted;
                while (true)
                {
                    if (copyTiming.WasExecuted)
                    {
                        CopyTimingToClipboard();
                    }

                    if (copyPosition.WasExecuted)
                    {
                        CopyPositionToClipboard();
                    }

                    if (copyX.WasExecuted)
                    {
                        CopyXToClipboard();
                    }

                    if (copyY.WasExecuted)
                    {
                        CopyYToClipboard();
                    }

                    if (restart.WasExecuted)
                    {
                        running = true;
                        break;
                    }

                    if ((close.WasExecuted && EventSystem.current.currentSelectedGameObject == null) || actionButtonPressed)
                    {
                        actionButtonPressed = false;
                        rect.gameObject.SetActive(false);
                        return;
                    }

                    await UniTask.NextFrame();
                }
            }
        }

        public void CloseMeasurer()
        {
            rect.gameObject.SetActive(false);
        }

        private void UpdateWindow(int timing)
        {
            currentTiming = timing;
            timingText.text = timing.ToString();

            Arc foundArc = null;
            for (int i = 0; i < selectedArcs.Count; i++)
            {
                Arc arc = selectedArcs[i];
                if (arc.Timing <= timing && timing <= arc.EndTiming)
                {
                    if (foundArc == null)
                    {
                        foundArc = arc;
                    }
                    else
                    {
                        // give up if there're two valid arcs
                        foundArc = null;
                        break;
                    }
                }
            }

            positionButton.gameObject.SetActive(foundArc != null);
            xButton.gameObject.SetActive(foundArc != null);
            yButton.gameObject.SetActive(foundArc != null);

            if (foundArc != null)
            {
                float x = foundArc.ArcXAt(timing);
                float y = foundArc.ArcYAt(timing);
                currentX = Mathf.Round(x * 1000) / 1000f;
                currentY = Mathf.Round(y * 1000) / 1000f;

                positionText.text = $"{currentX},{currentY}";
                xText.text = currentX.ToString();
                yText.text = currentY.ToString();
            }

            Vector2 mousePos = Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mousePos, null, out Vector2 rectPos);
            rect.anchoredPosition = rectPos + delta;
        }

        private void Awake()
        {
            timingButton.onClick.AddListener(CopyTimingToClipboard);
            positionButton.onClick.AddListener(CopyPositionToClipboard);
            xButton.onClick.AddListener(CopyXToClipboard);
            yButton.onClick.AddListener(CopyYToClipboard);
        }

        private void OnDestroy()
        {
            timingButton.onClick.RemoveListener(CopyTimingToClipboard);
            positionButton.onClick.RemoveListener(CopyPositionToClipboard);
            xButton.onClick.RemoveListener(CopyXToClipboard);
            yButton.onClick.RemoveListener(CopyYToClipboard);
        }

        private void CopyTimingToClipboard()
        {
            if (timingButton.gameObject.activeSelf)
            {
                GUIUtility.systemCopyBuffer = currentTiming.ToString();
                CloseMeasurer();
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Measurer.CopyTiming"));
                actionButtonPressed = true;
            }
        }

        private void CopyPositionToClipboard()
        {
            if (positionButton.gameObject.activeSelf)
            {
                GUIUtility.systemCopyBuffer = $"{currentX},{currentY}";
                CloseMeasurer();
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Measurer.CopyPosition"));
                actionButtonPressed = true;
            }
        }

        private void CopyXToClipboard()
        {
            if (xButton.gameObject.activeSelf)
            {
                GUIUtility.systemCopyBuffer = currentX.ToString();
                CloseMeasurer();
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Measurer.CopyX"));
                actionButtonPressed = true;
            }
        }

        private void CopyYToClipboard()
        {
            if (yButton.gameObject.activeSelf)
            {
                GUIUtility.systemCopyBuffer = currentY.ToString();
                CloseMeasurer();
                Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.Measurer.CopyY"));
                actionButtonPressed = true;
            }
        }
    }
}