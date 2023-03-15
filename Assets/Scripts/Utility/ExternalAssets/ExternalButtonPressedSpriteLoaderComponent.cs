using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Utility.ExternalAssets
{
    [RequireComponent(typeof(Button))]
    public class ExternalButtonPressedSpriteLoaderComponent : MonoBehaviour
    {
        [SerializeField] private string[] subdirectories;
        [SerializeField] private bool loadFullRect = false;

        private Button button;
        private ExternalSprite externalSprite;

        private void Awake()
        {
            button = GetComponent<Button>();

            string subdirectory = System.IO.Path.Combine(subdirectories);
            externalSprite = new ExternalSprite(button.spriteState.pressedSprite, subdirectory, loadFullRect);
            StartLoading().Forget();
        }

        private async UniTask StartLoading()
        {
            await externalSprite.Load();
            var state = button.spriteState;
            state.pressedSprite = externalSprite.Value;
            button.spriteState = state;
        }

        private void OnDestroy()
        {
            externalSprite.Unload();
            var state = button.spriteState;
            state.pressedSprite = externalSprite.Value;
            button.spriteState = state;
        }
    }
}