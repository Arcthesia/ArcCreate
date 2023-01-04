using UnityEngine;

namespace ArcCreate.Compose.Components
{
    public class OnFullscreen : MonoBehaviour
    {
        [SerializeField] private bool enableOnFullScreen;

        private void Awake()
        {
            Values.FullScreen.OnValueChange += OnFullScreenChange;
        }

        private void OnDestroy()
        {
            Values.FullScreen.OnValueChange -= OnFullScreenChange;
        }

        private void OnFullScreenChange(bool fullScreen)
        {
            if (fullScreen)
            {
                gameObject.SetActive(enableOnFullScreen);
            }
            else
            {
                gameObject.SetActive(!enableOnFullScreen);
            }
        }
    }
}