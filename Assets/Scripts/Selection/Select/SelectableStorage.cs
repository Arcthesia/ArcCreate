using System.Threading;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.InfiniteScroll;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Selection.Select
{
    public class SelectableStorage : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IEndDragHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Graphic selectedIndicator;
        [SerializeField] private Color defaultColor;
        [SerializeField] private int holdDurationMs = 1000;
        [SerializeField] private int blockClickDurationMs = 200;
        [SerializeField] private float dragCancelThreshold = 50;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private bool selected;
        private bool blockClick;
        private InfiniteScroll scroll;

        private IStorageUnit storageUnit;

        public IStorageUnit StorageUnit
        {
            get => storageUnit;
            set
            {
                storageUnit = value;
                enabled = storageUnit != null;
                SetSelected(value != null && Services.Select.IsStorageSelected(value));
                SetStateImmediate(selected);
                Cancel();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!selected)
            {
                Cancel();
                StartHoldDetection(cts.Token).Forget();
            }
        }

        public void OnPointerExit(PointerEventData eventData) => Cancel();

        public void OnEndDrag(PointerEventData eventData)
        {
            StartBlockClicking(cts.Token).Forget();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StartBlockClicking(cts.Token).Forget();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (blockClick)
            {
                return;
            }

            if (selected)
            {
                HideAnimation();
                SetSelected(false);
            }
            else if (Services.Select.IsAnySelected)
            {
                ShowAnimation();
                SetSelected(true);
            }
        }

        public void DeselectSelf()
        {
            if (selected)
            {
                HideAnimation();
                selected = false;
            }
        }

        private async UniTask StartHoldDetection(CancellationToken ct)
        {
            float startScrollValue = scroll.Value;
            UniTask<bool> waitTask = UniTask.Delay(holdDurationMs, cancellationToken: ct).SuppressCancellationThrow();

            while (waitTask.Status == UniTaskStatus.Pending)
            {
                await UniTask.NextFrame();
            }

            if (Mathf.Abs(scroll.Value - startScrollValue) >= dragCancelThreshold)
            {
                return;
            }

            bool cancelled = await waitTask;
            if (!cancelled)
            {
                blockClick = true;
                ShowAnimation();
                SetSelected(true);
            }
        }

        private async UniTask StartBlockClicking(CancellationToken ct)
        {
            await UniTask.Delay(blockClickDurationMs, cancellationToken: ct).SuppressCancellationThrow();
            blockClick = false;
        }

        private void ShowAnimation()
        {
            Color iconCol = defaultColor;
            iconCol.a = 0;
            selectedIndicator.color = iconCol;
            selectedIndicator.DOColor(defaultColor, 0.3f).SetEase(Ease.OutCubic);
        }

        private void HideAnimation()
        {
            Color iconCol = defaultColor;
            iconCol.a = 0;
            selectedIndicator.DOColor(iconCol, 0.3f).SetEase(Ease.OutCubic);
        }

        private void SetStateImmediate(bool selected)
        {
            selectedIndicator.DOKill();
            selectedIndicator.color = selected ? defaultColor : Color.clear;
        }

        private void SetSelected(bool selected)
        {
            this.selected = selected;
            if (selected)
            {
                Services.Select.Add(storageUnit);
            }
            else
            {
                Services.Select.Remove(storageUnit);
            }
        }

        private void Awake()
        {
            enabled = storageUnit != null;
            SetSelected(storageUnit != null && Services.Select.IsStorageSelected(storageUnit));
            Services.Select.OnClear += DeselectSelf;
            scroll = GetComponentInParent<InfiniteScroll>();
        }

        private void OnDestroy()
        {
            cts.Cancel();
            Services.Select.OnClear -= DeselectSelf;
        }

        private void OnDisable() => Cancel();

        private void Cancel()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }
    }
}