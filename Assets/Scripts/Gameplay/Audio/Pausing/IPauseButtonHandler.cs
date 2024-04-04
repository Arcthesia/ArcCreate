using UnityEngine.Events;

namespace ArcCreate.Gameplay.Audio
{
    public interface IPauseButtonHandler
    {
        void OnClick();

        void OnRelease();
    }
}