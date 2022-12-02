using TMPro;
using UnityEngine;

namespace ArcCreate.SceneTransition
{
    [RequireComponent(typeof(TMP_Text))]
    public class ReactiveText : MonoBehaviour
    {
        [SerializeField] private StringSO stringSO;
        private TMP_Text cachedText;

        protected virtual void OnTextChange(string text)
        {
            cachedText.text = text;
        }

        private void Awake()
        {
            cachedText = GetComponent<TMP_Text>();
            stringSO.OnValueChange.AddListener(OnTextChange);
        }

        private void OnDestroy()
        {
            stringSO.OnValueChange.RemoveListener(OnTextChange);
        }
    }
}