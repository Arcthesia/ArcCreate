using Cysharp.Threading.Tasks;

namespace ArcCreate.SceneTransition
{
    public class ShutterTransition : ITransition
    {
        private readonly Shutter shutter;

        public ShutterTransition()
        {
            shutter = Shutter.Instance;
        }

        public int WaitDurationMs => 0;

        public void DisableGameObject()
        {
            shutter.gameObject.SetActive(false);
        }

        public void EnableGameObject()
        {
            shutter.gameObject.SetActive(true);
        }

        public async UniTask StartTransition()
        {
            await shutter.Close(false);
        }

        public async UniTask EndTransition()
        {
            await shutter.Open();
        }
    }
}