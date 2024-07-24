using ArcCreate.Gameplay.Chart;

namespace ArcCreate.Compose.History
{
    public class AddTimingGroupCommand : ICommand
    {
        private readonly TimingGroup group;

        public AddTimingGroupCommand(
            string name,
            TimingGroup group)
        {
            this.group = group;
            Name = name;
        }

        public string Name { get; private set; }

        public void Execute()
        {
            Services.Gameplay.Chart.InsertTimingGroup(group);
        }

        public void Undo()
        {
            Services.Gameplay.Chart.RemoveTimingGroup(group);
        }
    }
}