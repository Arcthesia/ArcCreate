using Cysharp.Threading.Tasks;

namespace ArcCreate.SceneTransition
{
    public class SoundTransition : ITransition
    {
        private readonly TransitionScene.Sound sound;

        public SoundTransition(TransitionScene.Sound sound)
        {
            this.sound = sound;
        }

        public int DurationMs => 0;

        public void DisableGameObject()
        {
        }

        public void EnableGameObject()
        {
        }

        public UniTask EndTransition()
        {
            PlaySound();
            return default;
        }

        public UniTask StartTransition()
        {
            PlaySound();
            return default;
        }

        public void PlaySound()
        {
            if (TransitionScene.Instance != null)
            {
                TransitionScene.Instance.PlaySoundEffect(sound);
            }
        }
    }
}