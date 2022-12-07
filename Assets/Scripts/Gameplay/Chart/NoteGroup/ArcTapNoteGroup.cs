using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Chart
{
    public class ArcTapNoteGroup : ShortNoteGroup<ArcTap, ArcTapBehaviour>
    {
        public override string PoolName => Values.ArcTapPoolName;

        protected override void OnAdd(ArcTap note)
        {
        }

        protected override void OnUpdate(ArcTap note)
        {
        }

        protected override void OnRemove(ArcTap note)
        {
        }

        protected override void SetupNotes()
        {
        }
    }
}