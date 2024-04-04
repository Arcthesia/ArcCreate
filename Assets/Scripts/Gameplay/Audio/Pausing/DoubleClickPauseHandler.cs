using UnityEngine;

namespace ArcCreate.Gameplay.Audio
{
    public class DoubleClickPauseHandler : IPauseButtonHandler
    {
        private readonly PauseButton parent;
        private readonly float maxDuration;
        private float lastClickAt = float.MinValue;

        public DoubleClickPauseHandler(PauseButton parent, float maxDuration)
        {
            this.parent = parent;
            this.maxDuration = maxDuration;
        }

        public void OnClick()
        {
        }

        public void OnRelease()
        {
            float oldLast = lastClickAt;
            lastClickAt = Time.time;
            bool result = oldLast + maxDuration >= lastClickAt;
            if (result)
            {
                lastClickAt = float.MinValue;
                parent.Activate();
            }
        }
    }
}