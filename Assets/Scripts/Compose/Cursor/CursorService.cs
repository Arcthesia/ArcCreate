using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcCreate.Compose.Cursor
{
    public class CursorService : MonoBehaviour, ICursorService
    {
        [SerializeField] private MeshCollider laneCollider;
        [SerializeField] private Camera editorCamera;
        [SerializeField] private RectTransform gameplayViewport;

        private void Update()
        {
            if (Services.Gameplay?.IsLoaded ?? false)
            {
                CheckScroll();
            }
        }

        private void CheckScroll()
        {
            Mouse mouse = Mouse.current;
            float scrollY = mouse.scroll.ReadValue().y;
            if (scrollY == 0)
            {
                return;
            }

            Camera gameplayCamera = Services.Gameplay.Camera.GameplayCamera;
            Vector2 mousePosition = mouse.position.ReadValue();
            if (!RectTransformUtility.RectangleContainsScreenPoint(gameplayViewport, mousePosition, editorCamera))
            {
                return;
            }

            Ray ray = gameplayCamera.ScreenPointToRay(mousePosition);

            if (laneCollider.Raycast(ray, out RaycastHit hit, 120))
            {
                scrollY *= Settings.ScrollSensitivityVertical.Value;

                int timing = Services.Gameplay.Audio.ChartTiming;
                int snap = scrollY > 0 ? Services.Grid.MoveTimingForward(timing) : Services.Grid.MoveTimingBackward(timing);
                snap = Mathf.Clamp(snap, timing - Settings.TimingScrollMaxMovement.Value, timing + Settings.TimingScrollMaxMovement.Value);

                Services.Gameplay.Audio.ChartTiming = snap;
            }
        }
    }
}