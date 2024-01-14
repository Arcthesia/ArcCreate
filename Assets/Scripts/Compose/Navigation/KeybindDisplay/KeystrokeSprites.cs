using UnityEngine;

namespace ArcCreate.Compose.Navigation
{
    [CreateAssetMenu(fileName = "KeystrokeSprites", menuName = "ScriptableObject/KeystrokeSprites")]
    public class KeystrokeSprites : ScriptableObject
    {
        [SerializeField] private Sprite releaseContainerSprite;
        [SerializeField] private Sprite pressContainerSprite;
        [SerializeField] private Sprite leftMouseSprite;
        [SerializeField] private Sprite midMouseSprite;
        [SerializeField] private Sprite rightMouseSprite;
        [SerializeField] private Color releaseColor;
        [SerializeField] private Color pressColor;

        public Sprite ReleaseContainerSprite => releaseContainerSprite;

        public Sprite PressContainerSprite => pressContainerSprite;

        public Sprite LeftMouseSprite => leftMouseSprite;

        public Sprite MidMouseSprite => midMouseSprite;

        public Sprite RightMouseSprite => rightMouseSprite;

        public Color PressColor => pressColor;

        public Color ReleaseColor => releaseColor;
    }
}