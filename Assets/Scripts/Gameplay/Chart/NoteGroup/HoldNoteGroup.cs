using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Chart
{
    public class HoldNoteGroup : LongNoteGroup<Hold, HoldBehaviour>
    {
        public override string PoolName => "hold";

        public override void SetupNotes()
        {
        }

        protected override void OnAdd(Hold note)
        {
        }

        protected override void OnRemove(Hold note)
        {
        }

        protected override void OnUpdate(Hold note)
        {
        }
    }
}