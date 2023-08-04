using Cysharp.Threading.Tasks;

namespace ArcCreate.SceneTransition
{
    public class TriangleTileTransition : ITransition
    {
        private readonly bool inOutVariant;

        public TriangleTileTransition(bool inOutVariant = false)
        {
            this.inOutVariant = inOutVariant;
        }

        public int DurationMs => TransitionScene.Instance == null ? 0 : TransitionScene.Instance.TriangleTileAnimationDurationMs(inOutVariant);

        public void DisableGameObject()
        {
            if (TransitionScene.Instance != null)
            {
                TransitionScene.Instance.TriangleTileGameObject.SetActive(false);
            }
        }

        public void EnableGameObject()
        {
            if (TransitionScene.Instance != null)
            {
                TransitionScene.Instance.TriangleTileGameObject.SetActive(true);
            }
        }

        public async UniTask EndTransition()
        {
            if (TransitionScene.Instance != null)
            {
                await TransitionScene.Instance.HideTriangleTile(inOutVariant);
            }
        }

        public async UniTask StartTransition()
        {
            if (TransitionScene.Instance != null)
            {
                await TransitionScene.Instance.ShowTriangleTile(inOutVariant);
            }
        }
    }
}