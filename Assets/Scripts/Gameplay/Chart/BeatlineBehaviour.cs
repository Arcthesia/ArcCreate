using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    public class BeatlineBehaviour : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void SetColor(Color color)
        {
            spriteRenderer.color = color;
        }
    }
}