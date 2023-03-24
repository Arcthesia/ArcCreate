using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Utility.InfiniteScroll
{
    public abstract class Cell : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private bool useTwoStagesLoad;
        [SerializeField] private float predictedLoadTime;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public RectTransform RectTransform => rectTransform;

        public CellData CellData { get; set; }

        public HierarchyData HierarchyData { get; set; }

        public InfiniteScroll Scroll { get; set; }

        public float PredictedLoadTime => predictedLoadTime;

        /// <summary>
        /// Toggle collapse of this cell.
        /// </summary>
        public void ToggleCollapse()
        {
            Scroll.ToggleCollapse(HierarchyData.IndexFlat);
        }

        /// <summary>
        /// Add data to cell. Called whenever cells enter the viewport.
        /// </summary>
        /// <param name="cellData">The source data.</param>
        public abstract void SetCellData(CellData cellData);

        public async UniTask SetCellDataFully(CellData cellData)
        {
            if (useTwoStagesLoad)
            {
                CancelLoadCellFully();
                await LoadCellFully(cellData, cts.Token);
                HierarchyData.IsFullyLoaded = true;
            }
        }

        /// <summary>
        /// Finalize cell loading. Called when player scrolls slower than a set threshold for a set duration of time
        /// Intended for costly processes such as loading image texture.
        /// </summary>
        /// <param name="cellData">The source data.</param>
        /// <param name="cancellationToken">Cancellation token, invoked when cell exit the viewport.</param>
        /// <returns>UniTask instance.</returns>
        public abstract UniTask LoadCellFully(CellData cellData, CancellationToken cancellationToken);

        public void CancelLoadCellFully()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        public void MarkFullyLoaded()
        {
            HierarchyData.IsFullyLoaded = true;
        }
    }
}