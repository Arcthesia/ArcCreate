namespace ArcCreate.Compose.History
{
    public class CombinedCommand : ICommand
    {
        private readonly ICommand[] commands;

        public CombinedCommand(string name, params ICommand[] commands)
        {
            this.commands = commands;
            Name = name;
        }

        public string Name { get; private set; }

        public void Execute()
        {
            foreach (var cmd in commands)
            {
                cmd.Execute();
            }
        }

        public void Undo()
        {
            foreach (var cmd in commands)
            {
                cmd.Undo();
            }
        }
    }
}