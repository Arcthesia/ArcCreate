using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class RebuildLayoutOnEnable : MonoBehaviour
    {
        [SerializeField] private RectTransform layout;

        private void OnEnable()
        {
            Rebuild().Forget();
        }

        private async UniTask Rebuild()
        {
            await UniTask.NextFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
            layout.sizeDelta = new Vector2(layout.sizeDelta.x + 1, layout.sizeDelta.y + 1);
        }
    }
}