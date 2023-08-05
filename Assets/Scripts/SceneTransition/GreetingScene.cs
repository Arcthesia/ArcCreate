using ArcCreate.Utility.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.SceneTransition
{
    public class GreetingScene : SceneRepresentative
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject clickToStartText;
        [SerializeField] private ScriptedAnimator startupAnimator;
        [SerializeField] private ScriptedAnimator proceedAnimator;
        [SerializeField] private Camera greetingCamera;

        protected override void OnSceneLoad()
        {
            TransitionScene.Instance.SetTargetCamera(greetingCamera);
            TransitionScene.Instance.TriangleTileGameObject.SetActive(true);
            startupAnimator.Show();
            TransitionScene.Instance.EnterGreetingScene();
            button.onClick.AddListener(Transition);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(Transition);
        }

        private void Transition()
        {
            button.gameObject.SetActive(false);
            clickToStartText.SetActive(false);
            TransitionScene.Instance.EnterSelectScene();
            StartTransition().Forget();
        }

        private async UniTask StartTransition()
        {
            proceedAnimator.Hide();
            await UniTask.Delay((int)(proceedAnimator.Length * 1000));
            SceneTransitionManager.Instance.SwitchScene(SceneNames.SelectScene).Forget();
        }
    }
}