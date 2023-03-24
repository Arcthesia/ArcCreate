using System.Threading;
using ArcCreate.Storage.Data;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Selection.Select
{
    public class SelectableStorage : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Graphic selectedIndicator;
        [SerializeField] private Color defaultColor;
        [SerializeField] private int holdDurationMs = 1000;
        [SerializeField] private int blockClickDurationMs = 200;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private bool selected;
        private bool blockClick;

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
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!selected)
            {
                cts.Cancel();
                cts.Dispose();
                cts = new CancellationTokenSource();
                StartHoldDetection(cts.Token).Forget();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
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
            bool cancelled = await UniTask.Delay(holdDurationMs, cancellationToken: ct).SuppressCancellationThrow();
            if (!cancelled)
            {
                blockClick = true;
                ShowAnimation();
                SetSelected(true);
            }
        }

        private async UniTask StartBlockClicking(CancellationToken ct)
        {
            bool cancelled = await UniTask.Delay(blockClickDurationMs, cancellationToken: ct).SuppressCancellationThrow();
            if (!cancelled)
            {
                blockClick = false;
            }
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
        }

        private void OnDestroy()
        {
            cts.Cancel();
            Services.Select.OnClear -= DeselectSelf;
        }
    }
}