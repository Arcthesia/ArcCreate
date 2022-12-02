using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.SceneTransition
{
    [RequireComponent(typeof(Shutter))]
    public class ShutterWithInfoTransition : MonoBehaviour, ITransition
    {
        private Shutter shutter;

        public ShutterWithInfoTransition Instance { get; private set; }

        public int WaitDurationMs => Shutter.WaitBetween;

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
            await shutter.Close(true);
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