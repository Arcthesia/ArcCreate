using System.Collections.Generic;
using System.Linq;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Compose.History
{
    public class EventCommand : ICommand
    {
        private readonly bool addAvailable;
        private readonly bool removeAvailable;
        private readonly bool updateAvailable;
        private readonly List<ArcEvent> add;
        private readonly List<ArcEvent> remove;
        private readonly List<(ArcEvent instance, ArcEvent oldValue, ArcEvent newValue)> update;

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