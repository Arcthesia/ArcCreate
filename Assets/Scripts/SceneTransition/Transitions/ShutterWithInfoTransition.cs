using Cysharp.Threading.Tasks;

namespace ArcCreate.SceneTransition
{
    public class ShutterWithInfoTransition : ITransition
    {
        private readonly Shutter shutter;

        public ShutterWithInfoTransition()
        {
            shutter = Shutter.Instance;
        }

        public int WaitDurationMs => Shutter.WaitBetweenMs;

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
            await shutter.Close(true);
        }

        public async UniTask EndTransition()
        {
            await shutter.Open();
        }
    }
}