using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.SceneTransition
{
    [RequireComponent(typeof(Shutter))]
    public class ShutterTransition : MonoBehaviour, ITransition
    {
        private Shutter shutter;

        public ShutterTransition Instance { get; private set; }

        public int WaitDurationMs => 0;

        public void DisableGameObject()
        {
            gameObject.SetActive(false);
        }

        public void EnableGameObject()
        {
            gameObject.SetActive(true);
        }

        public async UniTask StartTransition()
        {
            await shutter.Close(false);
        }

        public async UniTask EndTransition()
        {
            await shutter.Open();
        }

        private void Awake()
        {
            Instance = this;
            shutter = GetComponent<Shutter>();
        }
    }
}