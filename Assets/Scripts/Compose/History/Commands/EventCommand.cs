using System.Collections.Generic;
using System.Linq;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.History
{
    /// <summary>
    /// Command for interacting with chart events.
    /// </summary>
    public class EventCommand : ICommand
    {
        private readonly bool addAvailable;
        private readonly bool removeAvailable;
        private readonly bool updateAvailable;
        private readonly List<ArcEvent> add;
        private readonly List<ArcEvent> remove;
        private readonly List<(ArcEvent instance, ArcEvent oldValue, ArcEvent newValue)> update;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCommand"/> class.
        /// </summary>
        /// <param name="name">The name of the command which will be displayed in log and notification.</param>
        /// <param name="add">Notes to add.</param>
        /// <param name="remove">Notes to remove.</param>
        /// <param name="update">List of update instructions.</param>
        /// <returns>A new <see cref="EventCommand"/> instance.</returns>
        public EventCommand(
            string name,
            IEnumerable<ArcEvent> add = null,
            IEnumerable<ArcEvent> remove = null,
            IEnumerable<(ArcEvent instance, ArcEvent newValue)> update = null)
        {
            Name = name;
            this.add = add?.ToList();
            this.remove = remove?.ToList();
            this.update = update?.Select(pair => (pair.instance, pair.instance.Clone(), pair.newValue)).ToList() ?? null;

            addAvailable = add?.Any() ?? false;
            removeAvailable = remove?.Any() ?? false;
            updateAvailable = update?.Any() ?? false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCommand"/> class.
        /// Create a batch command that executes multiple commands at once.
        /// </summary>
        /// <param name="name">The name of the command which will be displayed in log and notification.</param>
        /// <param name="commands">List of commands to include in the batch.</param>
        /// <returns>A command combining all included commands.</returns>
        public EventCommand(string name, IEnumerable<EventCommand> commands)
        {
            Name = name;
            add = new List<ArcEvent>();
            remove = new List<ArcEvent>();
            update = new List<(ArcEvent instance, ArcEvent oldValue, ArcEvent newValue)>();

            foreach (var cmd in commands)
            {
                if (cmd.add != null)
                {
                    add.AddRange(cmd.add);
                }

                if (cmd.remove != null)
                {
                    remove.AddRange(cmd.remove);
                }

                if (cmd.update != null)
                {
                    update.AddRange(cmd.update);
                }
            }

            addAvailable = add.Count > 0;
            removeAvailable = remove.Count > 0;
            updateAvailable = update.Count > 0;
        }

        public string Name { get; private set; }

        public void Execute()
        {
            if (addAvailable)
            {
                Services.Gameplay.Chart.AddEvents(add);
            }

            if (removeAvailable)
            {
                Services.Gameplay.Chart.RemoveEvents(remove);
            }

            if (updateAvailable)
            {
                foreach (var (instance, oldValue, newValue) in update)
                {
                    instance.Assign(newValue);
                }

                Services.Gameplay.Chart.UpdateEvents(update.Select(pair => pair.instance));
            }
        }

        public void Undo()
        {
            if (addAvailable)
            {
                Services.Gameplay.Chart.RemoveEvents(add);
            }

            if (removeAvailable)
            {
                Services.Gameplay.Chart.AddEvents(remove);
            }

            if (updateAvailable)
            {
                foreach (var (instance, oldValue, newValue) in update)
                {
                    instance.Assign(oldValue);
                }

                Services.Gameplay.Chart.UpdateEvents(update.Select(pair => pair.instance));
            }
        }
    }
}