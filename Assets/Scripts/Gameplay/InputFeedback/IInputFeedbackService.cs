namespace ArcCreate.Gameplay.InputFeedback
{
    /// <summary>
    /// Interface for providing hit effects services to internal (Gameplay) classes.
    /// </summary>
    public interface IInputFeedbackService
    {
        /// <summary>
        /// Update the visual.
        /// </summary>
        void UpdateInputFeedback();

        /// <summary>
        /// Darken the specified lane.
        /// Creates a new instance of darken sprite if none exists for the specified lane.
        /// </summary>
        /// <param name="lane">The lane to play the effect.</param>
        void LaneFeedback(int lane);

        /// <summary>
        /// Draw a horizontal line at the specified y value that exists for one frame.
        /// </summary>
        /// <param name="verticalY">The y coodinate of the line.</param>
        void FloatlineFeedback(float verticalY);
    }
}