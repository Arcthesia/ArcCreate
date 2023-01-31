using ArcCreate.Compose.Navigation;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    [EditorScope("Grid")]
    public class GridService : MonoBehaviour, IGridService
    {
        [SerializeField] private TimingGrid timingGrid;

        private bool isGridEnabled;

        public bool IsGridEnabled
        {
            get => isGridEnabled;
            set
            {
                isGridEnabled = value;
                timingGrid.SetGridEnabled(value);
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

        private void Awake()
        {
            timingGrid.SetGridEnabled(false);
            timingGrid.Setup();
        }
    }
}