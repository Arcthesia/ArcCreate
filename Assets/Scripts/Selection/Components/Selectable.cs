using System.Threading;
using ArcCreate.Storage.Data;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Selection.Components
{
    public class Selectable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Graphic icon;
        [SerializeField] private Transform selectedIcon;
        [SerializeField] private Color defaultBgColor;
        [SerializeField] private Color defaultIconColor;
        [SerializeField] private int holdDurationMs = 1000;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private bool selected;

        public IStorageUnit StorageUnit { get; set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!selected)
            {
                cts.Dispose();
                cts = new CancellationTokenSource();
                StartHoldDetection(cts.Token).Forget();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            cts.Cancel();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            cts.Cancel();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
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
            HideAnimation();
            selected = false;
        }

        private async UniTask StartHoldDetection(CancellationToken ct)
        {
            bool cancelled = await UniTask.Delay(holdDurationMs, cancellationToken: ct).SuppressCancellationThrow();
            if (!cancelled)
            {
                ShowAnimation();
                SetSelected(true);
            }
        }

        private void ShowAnimation()
        {
            Color bgCol = defaultBgColor;
            bgCol.a = 0;
            Color iconCol = defaultIconColor;
            iconCol.a = 0;
            background.color = bgCol;
            icon.color = iconCol;
            background.DOColor(defaultBgColor, 300).SetEase(Ease.OutCubic);
            icon.DOColor(defaultBgColor, 300).SetEase(Ease.OutCubic);
        }

        private void HideAnimation()
        {
            Color bgCol = defaultBgColor;
            bgCol.a = 0;
            Color iconCol = defaultIconColor;
            iconCol.a = 0;
            background.DOColor(bgCol, 300).SetEase(Ease.OutCubic);
            icon.DOColor(iconCol, 300).SetEase(Ease.OutCubic);
        }

        private void SetSelected(bool selected)
        {
            this.selected = selected;
            if (selected)
            {
                Services.Select.AddComponent(this);
            }
            else
            {
                Services.Select.RemoveComponent(this);
            }
        }

        private void OnDestroy()
        {
            cts.Cancel();
        }
    }
}