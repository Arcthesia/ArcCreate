using UnityEngine;

namespace ArcCreate.Compose.Navigation
{
    [CreateAssetMenu(fileName = "KeystrokeSprites", menuName = "ScriptableObject/KeystrokeSprites")]
    public class KeystrokeSprites : ScriptableObject
    {
        [SerializeField] private Sprite leftMouseSprite;
        [SerializeField] private Sprite midMouseSprite;
        [SerializeField] private Sprite rightMouseSprite;

        public Sprite LeftMouseSprite => leftMouseSprite;

        public Sprite MidMouseSprite => midMouseSprite;

        public Sprite RightMouseSprite => rightMouseSprite;
    }
}