using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.SceneTransition
{
    public class ReactiveTextWithTitle : ReactiveText
    {
        [SerializeField] private GameObject title;

        protected override void OnTextChange(string text)
        {
            base.OnTextChange(text);
            title.SetActive(!string.IsNullOrEmpty(text));
        }
    }
}