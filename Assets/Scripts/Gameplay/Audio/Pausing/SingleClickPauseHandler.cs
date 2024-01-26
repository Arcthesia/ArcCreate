namespace ArcCreate.Gameplay.Audio
{
    public class SingleClickPauseHandler : IPauseButtonHandler
    {
        private readonly PauseButton parent;

        public SingleClickPauseHandler(PauseButton parent)
        {
            this.parent = parent;
        }

        public void OnClick()
        {
            parent.Activate();
        }

        public void OnRelease()
        {
        }
    }
}