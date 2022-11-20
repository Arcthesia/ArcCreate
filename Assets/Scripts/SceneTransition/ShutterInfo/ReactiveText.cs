using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.SceneTransition
{
    [RequireComponent(typeof(Text))]
    public class ReactiveText : MonoBehaviour
    {
        [SerializeField] private StringSO stringSO;
        [SerializeField] private Text cachedText;

        protected virtual void OnTextChange(string text)
        {
            cachedText.text = text;
        }

        private void Awake()
        {
            cachedText = GetComponent<Text>();
            stringSO.OnValueChange.AddListener(OnTextChange);
        }

        private void OnDestroy()
        {
            stringSO.OnValueChange.RemoveListener(OnTextChange);
        }
    }
}