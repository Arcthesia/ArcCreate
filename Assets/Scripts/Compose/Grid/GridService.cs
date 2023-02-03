using ArcCreate.Compose.Navigation;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    [EditorScope("Grid")]
    public class GridService : MonoBehaviour, IGridService
    {
        [SerializeField] private TimingGrid timingGrid;
        [SerializeField] private VerticalGrid verticalGrid;

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

        [EditorAction("Toggle", true, "g")]
        [RequireGameplayLoaded]
        public void ToggleGrid()
        {
            IsGridEnabled = !IsGridEnabled;
        }

        public int MoveTimingBackward(int sourceTiming) => timingGrid.MoveTimingBackward(sourceTiming);

        public int MoveTimingForward(int sourceTiming) => timingGrid.MoveTimingForward(sourceTiming);

        public int SnapTimingToGrid(int sourceTiming) => timingGrid.SnapToTimingGrid(sourceTiming);

        public int SnapTimingToGridIfGridIsEnabled(int sourceTiming) => IsGridEnabled ? SnapTimingToGrid(sourceTiming) : sourceTiming;

        public Vector2 SnapPointToGridIfEnabled(Vector2 point) => IsGridEnabled ? SnapPointToGrid(point) : point;

        public Vector2 SnapPointToGrid(Vector2 point) => verticalGrid.SnapToVerticalGrid(point);

        private void Awake()
        {
            IsGridEnabled = false;
            timingGrid.Setup();

            string testScript = @"
            grid.setCollider(-0.5, 1.5, 0, 1)
            grid.setPanelColor(rgba(0, 0, 0, 0))
            grid.drawLine(0.25, 0.25, 0, 1)
            grid.drawLine(-0.5, 1.5, 0.5, 0.5)
            grid.drawLine(-0.5, 1.5, 1, 0, hsva(360, 1, 1, 1))

            grid.drawArea(rgba(200, 0, 0, 32), xy(-0.5, 0), xy(-0.5, 1), xy(-0.25, 1))
            grid.drawArea(rgba(200, 200, 0, 32), xy(-0.5, 0), xy(-0.25, 1), xy(0, 1))

            grid.drawArea(rgba(200, 0, 0, 32), xy(1.5, 0), xy(1.5, 1), xy(1.25, 1))
            grid.drawArea(rgba(200, 200, 0, 32), xy(1.5, 0), xy(1.25, 1), xy(1, 1))

            grid.drawArea(rgba(0, 200, 0, 32), xy(-0.5, 0), xy(0, 1), xy(1, 1), xy(1.5, 0))
            ";

            IGridSettings settings = new LuaGridSettings(testScript);

            verticalGrid.LoadGridSettings(settings.ColliderArea, settings.PanelColor, settings.SnapTolerance, settings.Lines, settings.Areas);
            timingGrid.LoadGridSettings(Settings.LaneFrom.Value, Settings.LaneTo.Value);
        }
    }
}