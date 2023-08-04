using Cysharp.Threading.Tasks;

namespace ArcCreate.SceneTransition
{
    public class PlayRetryCountTransition : ITransition
    {
        public int DurationMs => TransitionScene.Instance == null ? 0 : TransitionScene.Instance.PlayRetryCountAnimationDurationMs;

        public void DisableGameObject()
        {
            if (TransitionScene.Instance != null)
            {
                TransitionScene.Instance.PlayRetryCountGameObject.SetActive(false);
            }
        }

        public void EnableGameObject()
        {
            if (TransitionScene.Instance != null)
            {
                TransitionScene.Instance.PlayRetryCountGameObject.SetActive(true);
            }
        }

        public async UniTask EndTransition()
        {
            if (TransitionScene.Instance != null)
            {
                await TransitionScene.Instance.HidePlayRetryCount();
            }
        }

        public async UniTask StartTransition()
        {
            if (TransitionScene.Instance != null)
            {
                await TransitionScene.Instance.ShowPlayRetryCount();
            }
        }
    }
}