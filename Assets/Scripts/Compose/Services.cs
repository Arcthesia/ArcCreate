using ArcCreate.Compose.Cursor;
using ArcCreate.Compose.EventsEditor;
using ArcCreate.Compose.Grid;
using ArcCreate.Compose.History;
using ArcCreate.Compose.Macros;
using ArcCreate.Compose.Navigation;
using ArcCreate.Compose.Popups;
using ArcCreate.Compose.Project;
using ArcCreate.Compose.Selection;
using ArcCreate.Compose.Timeline;
using ArcCreate.Gameplay;
using ArcCreate.Utility.Lua;
using UnityEngine;

namespace ArcCreate.Compose
{
    internal class Services : MonoBehaviour
    {
        [SerializeField] private ProjectService project;
        [SerializeField] private PopupsService popups;
        [SerializeField] private TimelineService timeline;
        [SerializeField] private HistoryService history;
        [SerializeField] private NavigationService navigation;
        [SerializeField] private CursorService cursor;
        [SerializeField] private GridService grid;
        [SerializeField] private SelectionService selection;
        [SerializeField] private MacroService macros;
        [SerializeField] private ScenecontrolDebugService scDbg;

        public static IProjectService Project { get; private set; }

        public static IPopupsService Popups { get; private set; }

        public static ITimelineService Timeline { get; private set; }

        public static IGameplayControl Gameplay { get; set; }
        
        public static IScriptDebugSetup ScenecontrolDebug { get; set; }

        public static IHistoryService History { get; set; }

        public static INavigationService Navigation { get; set; }

        public static ICursorService Cursor { get; set; }

        public static IGridService Grid { get; set; }

        public static ISelectionService Selection { get; set; }

        public static IMacroService Macros { get; set; }

        private void Awake()
        {
            Project = project;
            Popups = popups;
            Timeline = timeline;
            History = history;
            Navigation = navigation;
            Cursor = cursor;
            Grid = grid;
            Selection = selection;
            Macros = macros;
            ScenecontrolDebug = scDbg;
        }
    }
}