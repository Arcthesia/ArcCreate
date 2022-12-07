using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Chart
{
    public class TapNoteGroup : ShortNoteGroup<Tap, TapBehaviour>
    {
        public override string PoolName => Values.TapPoolName;

        protected override void OnAdd(Tap note)
        {
        }

        protected override void OnUpdate(Tap note)
        {
        }

        protected override void OnRemove(Tap note)
        {
        }

        protected override void SetupNotes()
        {
        }
    }
}