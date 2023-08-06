using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Selection.SoundEffect
{
    [RequireComponent(typeof(Button))]
    public class PlaySoundButton : MonoBehaviour
    {
        [SerializeField] private Sound sound;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(PlaySound);
        }

        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(PlaySound);
        }

        private void PlaySound()
        {
            Services.SoundEffect.Play(sound);
        }
    }
}