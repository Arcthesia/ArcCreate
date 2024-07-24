using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.History
{
    public class EditTimingGroupProperitesCommand : ICommand
    {
        private readonly TimingGroup group;
        private readonly GroupProperties oldProperties;
        private readonly GroupProperties newProperties;

        public EditTimingGroupProperitesCommand(
            string name,
            TimingGroup group,
            GroupProperties properties)
        {
            this.group = group;
            this.oldProperties = group.GroupProperties;
            this.newProperties = properties;
            Name = name;
        }

        public string Name { get; private set; }

        public void Execute()
        {
            group.SetGroupProperties(newProperties);
        }

        public void Undo()
        {
            group.SetGroupProperties(oldProperties);
        }
    }
}