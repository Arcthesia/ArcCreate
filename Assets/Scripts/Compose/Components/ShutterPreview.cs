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

        private TransitionSequence transition;

        private TransitionSequence Transition
        {
            get
            {
                transition = transition ?? new TransitionSequence()
                    .OnBoth()
                    .AddTransition(new InfoTransition())
                    .AddTransition(new DecorationTransition())
                    .AddTransition(new TriangleTileTransition());

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
            Transition.Show().Forget();
            showButton.gameObject.SetActive(false);
            hideButton.gameObject.SetActive(true);
        }

        private void HideShutter()
        {
            Transition.Hide().Forget();
            showButton.gameObject.SetActive(true);
            hideButton.gameObject.SetActive(false);
        }
    }
}