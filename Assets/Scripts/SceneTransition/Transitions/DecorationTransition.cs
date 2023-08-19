using Cysharp.Threading.Tasks;

namespace ArcCreate.SceneTransition
{
    public class DecorationTransition : ITransition
    {
        public int DurationMs => TransitionScene.Instance == null ? 0 : TransitionScene.Instance.DecorationAnimationDurationMs;

        public void DisableGameObject()
        {
            if (TransitionScene.Instance != null)
            {
                TransitionScene.Instance.DecorationGameObject.SetActive(false);
                TransitionScene.Instance.UpdateCameraStatus();
            }
        }

        public void EnableGameObject()
        {
            if (TransitionScene.Instance != null)
            {
                TransitionScene.Instance.DecorationGameObject.SetActive(true);
                TransitionScene.Instance.UpdateCameraStatus();
            }
        }

        public async UniTask EndTransition()
        {
            if (TransitionScene.Instance != null)
            {
                await TransitionScene.Instance.HideDecoration();
            }
        }

        public async UniTask StartTransition()
        {
            if (TransitionScene.Instance != null)
            {
                await TransitionScene.Instance.ShowDecoration();
            }
        }
    }
}