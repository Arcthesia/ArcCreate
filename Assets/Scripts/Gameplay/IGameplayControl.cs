namespace ArcCreate.Gameplay
{
    /// <summary>
    /// Boundary interface of Gameplay module.
    /// </summary>
    public interface IGameplayControl
    {
        /// <summary>
        /// Gets the controller for chart related functionality.
        /// </summary>
        /// <value>The chart controller.</value>
        IChartControl Chart { get; }

        /// <summary>
        /// Gets the controller for skin related functionality.
        /// </summary>
        /// <value>The skin controller.</value>
        ISkinControl Skin { get; }
    }
}