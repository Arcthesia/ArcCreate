using System.Collections.Generic;

namespace ArcCreate.Compose.Navigation
{
    public class CompositeAction : IAction
    {
        private readonly List<IAction> actions;

        public CompositeAction(string id, List<IAction> actions)
        {
            Id = id;
            this.actions = actions;
        }

        public string Id { get; private set; }

        public string FullPath => Id;

        public string CategoryI18nName => null;

        public string I18nName => null;

        public bool ShouldDisplayOnContextMenu => false;

        public void Execute()
        {
            Services.Navigation.StartActionsInSequence(actions);
        }
    }
}