using ArcCreate.SceneTransition;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose.Components
{
    public class ShutterPreview : MonoBehaviour
    {
        [SerializeField] private Button showButton;
        [SerializeField] private Button hideButton;

        private ITransition transition;

        private ITransition Transition
        {
            get
            {
                transition = transition ?? new ShutterWithInfoTransition();

                return transition;
            }
        }

        private void Awake()
        {
            showButton.onClick.AddListener(ShowShutter);
            hideButton.onClick.AddListener(HideShutter);
            hideButton.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            showButton.onClick.RemoveListener(ShowShutter);
            hideButton.onClick.RemoveListener(HideShutter);
        }

        private void ShowShutter()
        {
            Transition.EnableGameObject();
            Transition.StartTransition();
            showButton.gameObject.SetActive(false);
            hideButton.gameObject.SetActive(true);
        }

        private void HideShutter()
        {
            Transition.EndTransition().ContinueWith(Transition.DisableGameObject);
            showButton.gameObject.SetActive(true);
            hideButton.gameObject.SetActive(false);
        }
    }
}