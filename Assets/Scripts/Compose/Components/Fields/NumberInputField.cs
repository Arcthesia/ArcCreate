using System.Threading;
using ArcCreate.Utility.Parser;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    /// <summary>
    /// Component for number input fields.
    /// Provides an increase and decrease button.
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class NumberInputField : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TMP_InputField inputField;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private float increment;
        [SerializeField] private int debounceMs = 1000;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public void OnPointerEnter(PointerEventData eventData)
        {
            increaseButton.gameObject.SetActive(true);
            decreaseButton.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            increaseButton.gameObject.SetActive(false);
            decreaseButton.gameObject.SetActive(false);
        }

        private void Increment()
        {
            if (Evaluator.TryFloat(inputField.text, out float val))
            {
                val += increment;
                string str = val.ToString();
                inputField.text = str;
                cts.Cancel();
                cts.Dispose();
                cts = new CancellationTokenSource();
                StartNotifying(cts.Token, str).Forget();
            }
        }

        private void Decrement()
        {
            if (Evaluator.TryFloat(inputField.text, out float val))
            {
                val -= increment;
                string str = val.ToString();
                inputField.text = str;
                cts.Cancel();
                cts.Dispose();
                cts = new CancellationTokenSource();
                StartNotifying(cts.Token, str).Forget();
            }
        }

        private async UniTask StartNotifying(CancellationToken ct, string str)
        {
            bool isCancelled = await UniTask.Delay(debounceMs, cancellationToken: ct).SuppressCancellationThrow();
            if (!isCancelled)
            {
                inputField.onEndEdit.Invoke(str);
            }
        }

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
            increaseButton.onClick.AddListener(Increment);
            decreaseButton.onClick.AddListener(Decrement);
            increaseButton.gameObject.SetActive(false);
            decreaseButton.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            increaseButton.onClick.RemoveAllListeners();
            decreaseButton.onClick.RemoveAllListeners();
        }
    }
}