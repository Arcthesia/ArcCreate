namespace ArcCreate.Compose.Navigation
{
    public class SubAction : IAction
    {
        private bool executed = false;

        public SubAction(string displayName, bool shouldDisplayOnContextMenu)
        {
            DisplayName = displayName;
            ShouldDisplayOnContextMenu = shouldDisplayOnContextMenu;
        }

        /// <summary>
        /// Gets a value indicating whether or not this sub-action was executed.
        /// Reading this value will reset it to false.
        /// </summary>
        /// <value>Whether or not this sub-action was executed.</value>
        public bool WasExecuted
        {
            get
            {
                bool output = executed;
                executed = false;
                return output;
            }
        }

        public string DisplayName { get; private set; }

        public bool ShouldDisplayOnContextMenu { get; private set; }

        public void Execute()
        {
            executed = true;
        }
    }
}