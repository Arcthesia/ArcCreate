using Cysharp.Threading.Tasks;

namespace ArcCreate.SceneTransition
{
    public interface ITransition
    {
        int WaitDurationMs { get; }

        void EnableGameObject();

        void DisableGameObject();

        UniTask StartTransition();

        UniTask EndTransition();
    }
}