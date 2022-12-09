using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Chart
{
    public class ArcNoteGroup : LongNoteGroup<Arc, ArcBehaviour>
    {
        public override string PoolName => Values.ArcPoolName;

        protected override void OnAdd(Arc note)
        {
            ChainArcIntoGroups(note);
        }

        protected override void OnUpdate(Arc note)
        {
            RemoveArcFromChainGroups(note);
            ChainArcIntoGroups(note);
        }

        protected override void OnRemove(Arc note)
        {
            RemoveArcFromChainGroups(note);
        }

        protected override void SetupNotes()
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                Arc arc = Notes[i];
                ChainArcIntoGroups(arc);
            }
        }

        private void ChainArcIntoGroups(Arc arc)
        {
            var overlapStart = TimingTree[arc.Timing, arc.Timing];
            var overlapEnd = TimingTree[arc.EndTiming, arc.EndTiming];

            while (overlapStart.MoveNext())
            {
                Arc overlap = overlapStart.Current;
                if (IsChained(overlap, arc))
                {
                    arc.PreviousArcs.Add(overlap);
                    overlap.NextArcs.Add(arc);
                }
            }

            while (overlapEnd.MoveNext())
            {
                Arc overlap = overlapEnd.Current;
                if (IsChained(arc, overlap))
                {
                    arc.NextArcs.Add(overlap);
                    overlap.PreviousArcs.Add(arc);
                }
            }
        }

        private void RemoveArcFromChainGroups(Arc arc)
        {
            foreach (var other in arc.NextArcs)
            {
                other.PreviousArcs.Remove(arc);
            }

            foreach (var other in arc.PreviousArcs)
            {
                other.NextArcs.Remove(arc);
            }

            arc.NextArcs.Clear();
            arc.PreviousArcs.Clear();
        }

        private bool IsChained(Arc first, Arc second)
        {
            return
                first.EndTiming == second.Timing
             && first.XEnd == second.XStart
             && first.YEnd == second.YStart
             && first.IsTrace == second.IsTrace;
        }
    }
}