using Cysharp.Threading.Tasks;

namespace ArcCreate.SceneTransition
{
    public class InfoTransition : ITransition
    {
        public int DurationMs => TransitionScene.Instance == null ? 0 : TransitionScene.Instance.InfoAnimationDurationMs;

        public void DisableGameObject()
        {
            if (TransitionScene.Instance != null)
            {
                TransitionScene.Instance.InfoGameObject.SetActive(false);
            }
        }

        public void EnableGameObject()
        {
            if (TransitionScene.Instance != null)
            {
                TransitionScene.Instance.InfoGameObject.SetActive(true);
            }
        }

        public async UniTask EndTransition()
        {
            if (TransitionScene.Instance != null)
            {
                await TransitionScene.Instance.HideInfo();
            }
        }

        public async UniTask StartTransition()
        {
            if (TransitionScene.Instance != null)
            {
                await TransitionScene.Instance.ShowInfo();
            }
        }
    }
}