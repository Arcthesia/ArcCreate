using ArcCreate.Utility.Parser;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    [RequireComponent(typeof(TMP_InputField))]
    public class NumberInputField : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TMP_InputField inputField;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private int increment;

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
            Evaluator.TryFloat(inputField.text, out float val);
            val += increment;
            inputField.text = val.ToString();
        }

        private void Decrement()
        {
            Evaluator.TryFloat(inputField.text, out float val);
            val -= increment;
            inputField.text = val.ToString();
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