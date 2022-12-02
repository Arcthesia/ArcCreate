using UnityEngine;

namespace ArcCreate.Gameplay.InputFeedback
{
    /// <summary>
    /// Interface for providing hit effects services to internal (Gameplay) classes.
    /// </summary>
    public interface IInputFeedbackService
    {
        void UpdateInputFeedback();

        void LaneFeedback(int lane);

        void FloatlineFeedback(float verticalY);
    }
}