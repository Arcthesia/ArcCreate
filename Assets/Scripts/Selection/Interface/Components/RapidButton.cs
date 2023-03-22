using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Selection.Interface
{
    [RequireComponent(typeof(Button))]
    public class RapidButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private int startingCount = 5;
        [SerializeField] private int decreaseRate = 1;
        private Button button;

        private CancellationTokenSource cts = new CancellationTokenSource();

        public void OnPointerDown(PointerEventData eventData)
        {
            HeldTask(cts.Token).Forget();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private async UniTask HeldTask(CancellationToken ct)
        {
            int count = startingCount;
            while (true)
            {
                await UniTask.DelayFrame(count);
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                count = Mathf.Max(1, count - decreaseRate);
                button.onClick?.Invoke();
            }
        }
    }
}