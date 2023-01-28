namespace ArcCreate.Compose.Navigation
{
    public interface IContextRequirement
    {
        /// <summary>
        /// Check if the requirement has been satisfied.
        /// </summary>
        /// <returns>Whether or not the requirement has been satisfied.</returns>
        bool CheckRequirement();
    }
}