using ArcCreate.Gameplay.Chart;

namespace ArcCreate.Compose.History
{
    public class RemoveTimingGroupCommand : ICommand
    {
        private readonly TimingGroup group;

        public RemoveTimingGroupCommand(
            string name,
            TimingGroup group)
        {
            this.group = group;
            Name = name;
        }

        public string Name { get; private set; }

        public void Execute()
        {
            Services.Gameplay.Chart.RemoveTimingGroup(group);
        }

        public void Undo()
        {
            Services.Gameplay.Chart.InsertTimingGroup(group);
        }
    }
}