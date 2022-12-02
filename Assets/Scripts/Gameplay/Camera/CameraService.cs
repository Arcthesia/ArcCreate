using UnityEngine;

namespace ArcCreate.Gameplay.GameplayCamera
{
    public class CameraService : MonoBehaviour, ICameraService
    {
        [SerializeField] private Camera gameplayCamera;

        public Camera GameplayCamera => gameplayCamera;
    }
}