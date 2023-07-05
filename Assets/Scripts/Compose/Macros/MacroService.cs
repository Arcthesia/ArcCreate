using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArcCreate.Compose.Navigation;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility;
using ArcCreate.Utility.InfiniteScroll;
using ArcCreate.Utility.Lua;
using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Macros
{
    [EditorScope("Macros")]
    public class MacroService : MonoBehaviour, IMacroService
    {
        [SerializeField] private MacroDialog macroDialog;
        [SerializeField] private MacroPicker macroPicker;
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button generateEmmyLuaButton;
        [SerializeField] private Button openExplorerButton;

        [SerializeField] private Button showFullListButton;
        [SerializeField] private Button hideFullListButton;
        [SerializeField] private GameObject fullMacroList;

        [SerializeField] private Transform macroCellParent;
        [SerializeField] private GameObject macroCellPrefab;
        [SerializeField] private float macroCellSize;

        private Pool<Cell> macroCellPool;

        private (EventSelectionConstraint constraint, EventSelectionRequest request, bool selectSingle) currentSelectionRequest;
        private (MacroRequest request, int timing) currentMacroRequest;

        private MacroLuaEnvironment macroEnvironment;

        public void RunMacro(string macroId)
        {
            if (!macroEnvironment.TryGetMacro(macroId, out MacroDefinition macro))
            {
                Services.Popups.Notify(Popups.Severity.Error, I18n.S("Compose.Notift.Macros.NotFound", macroId));
                return;
            }

            macroEnvironment.RunMacro(macro).Forget();
            macroPicker.SetLastRunMacro(macro);
        }

        public void CreateDialog(string dialogTitle, DialogField[] fields, MacroRequest request)
        {
            macroDialog.SetTitle(dialogTitle);
            macroDialog.CreateFields(fields, request);
            macroDialog.Open();
            macroEnvironment.WaitForRequest(request);
        }

        public void RequestSelection(EventSelectionConstraint constraint, EventSelectionRequest request, bool selectSingle)
        {
            currentSelectionRequest = (constraint, request, selectSingle);
            Services.Navigation.StartAction("Macros.SelectEvents");
            macroEnvironment.WaitForRequest(request);
        }

        [EditorAction("SelectEvents", false)]
        [SubAction("Confirm", false, "<cr>")]
        [SubAction("Cancel", false, "<esc>")]
        [WhitelistScopes(typeof(Timeline.TimelineService), typeof(Selection.SelectionService))]
        public async UniTask SelectEventsAction(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");
            var (constraint, request, selectSingle) = currentSelectionRequest;
            bool complete = false;
            HashSet<Note> latestValidSelection = new HashSet<Note>();

            Services.Selection.OnSelectionChange += OnSelectionChange;
            void OnSelectionChange(HashSet<Note> notes)
            {
                Dictionary<string, List<LuaChartEvent>> events = Event.ConvertAll(notes);
                foreach (var list in events.Values)
                {
                    foreach (var e in list)
                    {
                        (bool valid, string invalidReason) = constraint.GetValidityDetail(e);
                        if (!valid)
                        {
                            Services.Popups.Notify(Popups.Severity.Warning, invalidReason);
                            Services.Selection.SetSelection(latestValidSelection);
                            return;
                        }
                    }
                }

                latestValidSelection = new HashSet<Note>(notes);
                if (selectSingle && notes.Count == 1)
                {
                    complete = true;
                }
            }

            OnSelectionChange(Services.Selection.SelectedNotes);
            while (!complete)
            {
                if (cancel.WasExecuted)
                {
                    macroEnvironment.CancelMacro();
                    Services.Selection.OnSelectionChange -= OnSelectionChange;
                    return;
                }

                if (confirm.WasExecuted && latestValidSelection.Count > 0)
                {
                    break;
                }

                await UniTask.NextFrame();
            }

            request.Result = Event.ConvertAll(latestValidSelection);
            request.Complete = true;
            Services.Selection.OnSelectionChange -= OnSelectionChange;
            Services.Selection.SetSelection(Enumerable.Empty<Note>());
        }

        public void RequestTrackLane(MacroRequest request)
        {
            currentMacroRequest = (request, 0);
            Services.Navigation.StartAction("Macros.SelectLane");
            macroEnvironment.WaitForRequest(request);
        }

        [EditorAction("SelectLane", false)]
        [SubAction("Confirm", false, "<mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [WhitelistScopes(typeof(Timeline.TimelineService), typeof(Grid.GridService), typeof(Cursor.CursorService))]
        public async UniTask SelectLaneAction(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");
            var (request, _) = currentMacroRequest;

            (bool success, int lane) = await Services.Cursor.RequestLaneSelection(confirm, cancel);
            if (!success)
            {
                macroEnvironment.CancelMacro();
                return;
            }

            request.Result["lane"] = DynValue.NewNumber(lane);
            request.Complete = true;
        }

        public void RequestTrackPosition(MacroRequest request, int timing)
        {
            currentMacroRequest = (request, timing);
            Services.Navigation.StartAction("Macros.SelectPosition");
            macroEnvironment.WaitForRequest(request);
        }

        [EditorAction("SelectPosition", false)]
        [SubAction("Confirm", false, "<mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [WhitelistScopes(typeof(Timeline.TimelineService), typeof(Grid.GridService), typeof(Cursor.CursorService))]
        public async UniTask SelectPositionAction(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");
            var (request, timing) = currentMacroRequest;

            (bool success, Vector2 position) = await Services.Cursor.RequestVerticalSelection(confirm, cancel, timing);
            if (!success)
            {
                macroEnvironment.CancelMacro();
                return;
            }

            request.Result["x"] = DynValue.NewNumber(position.x);
            request.Result["y"] = DynValue.NewNumber(position.y);
            request.Result["xy"] = UserData.Create(new XY(position));
            request.Complete = true;
        }

        public void RequestTrackTiming(MacroRequest request)
        {
            currentMacroRequest = (request, 0);
            Services.Navigation.StartAction("Macros.SelectTiming");
            macroEnvironment.WaitForRequest(request);
        }

        [EditorAction("SelectTiming", false)]
        [SubAction("Confirm", false, "<mouse1>")]
        [SubAction("Cancel", false, "<esc>")]
        [WhitelistScopes(typeof(Timeline.TimelineService), typeof(Grid.GridService), typeof(Cursor.CursorService))]
        public async UniTask SelectTimingAction(EditorAction action)
        {
            SubAction confirm = action.GetSubAction("Confirm");
            SubAction cancel = action.GetSubAction("Cancel");
            var (request, _) = currentMacroRequest;

            (bool success, int timing) = await Services.Cursor.RequestTimingSelection(confirm, cancel);
            if (!success)
            {
                macroEnvironment.CancelMacro();
                return;
            }

            request.Result["timing"] = DynValue.NewNumber(timing);
            request.Complete = true;
        }

        private void Awake()
        {
            macroCellPool = new Pool<Cell>(macroCellPrefab, macroCellParent, 5);
            macroEnvironment = new MacroLuaEnvironment(macroPicker, macroCellPool, macroCellSize);
            reloadButton.onClick.AddListener(Reload);
            generateEmmyLuaButton.onClick.AddListener(GenerateEmmyLua);
            openExplorerButton.onClick.AddListener(OpenExplorer);
            showFullListButton.onClick.AddListener(ShowFullList);
            hideFullListButton.onClick.AddListener(HideFullList);
            macroEnvironment.ReloadMacros();
        }

        private void OnDestroy()
        {
            macroCellPool.Destroy();
            reloadButton.onClick.RemoveListener(Reload);
            generateEmmyLuaButton.onClick.RemoveListener(GenerateEmmyLua);
            openExplorerButton.onClick.RemoveListener(OpenExplorer);
            showFullListButton.onClick.AddListener(ShowFullList);
            hideFullListButton.onClick.AddListener(HideFullList);
            macroEnvironment.CancelMacro();
        }

        private void ShowFullList()
        {
            fullMacroList.SetActive(true);
        }

        private void HideFullList()
        {
            fullMacroList.SetActive(false);
        }

        private void OpenExplorer()
        {
            if (!Directory.Exists(macroEnvironment.MacroDefFolder))
            {
                Directory.CreateDirectory(macroEnvironment.MacroDefFolder);
            }

            Shell.OpenExplorer(macroEnvironment.MacroDefFolder);
        }

        private void GenerateEmmyLua()
        {
            macroEnvironment.GenerateEmmyLua();
            Services.Popups.Notify(Popups.Severity.Info, I18n.S("Compose.Notify.EmmyLuaGenerated.Macros"));
        }

        private void Reload()
        {
            macroEnvironment.ReloadMacros();
        }
    }
}