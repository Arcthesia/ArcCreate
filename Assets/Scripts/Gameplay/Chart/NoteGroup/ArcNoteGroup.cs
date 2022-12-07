using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Chart
{
    public class ArcNoteGroup : LongNoteGroup<Arc, ArcBehaviour>
    {
        public override string PoolName => Values.ArcPoolName;

        protected override void OnAdd(Arc note)
        {
        }

        protected override void OnUpdate(Arc note)
        {
        }

        protected override void OnRemove(Arc note)
        {
        }

        protected override void SetupNotes()
        {
        }
    }
}