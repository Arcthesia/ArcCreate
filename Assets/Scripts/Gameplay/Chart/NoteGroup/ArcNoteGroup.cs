using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    public class ArcNoteGroup : LongNoteGroup<Arc>
    {
        public override void SetupNotes()
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                Arc arc = Notes[i];
                ChainArcIntoGroups(arc);
            }
        }

        public override void Clear()
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                Arc arc = Notes[i];
                arc.CleanColliderMesh();
            }

            base.Clear();
        }

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

        private void ChainArcIntoGroups(Arc arc)
        {
            foreach (Arc overlap in Services.Chart.FindByEndTiming<Arc>(arc.Timing - 1, arc.Timing + 1))
            {
                if (IsChained(overlap, arc))
                {
                    arc.PreviousArcs.Add(overlap);
                    overlap.NextArcs.Add(arc);
                }
            }

            foreach (Arc overlap in Services.Chart.FindByTiming<Arc>(arc.EndTiming - 1, arc.EndTiming + 1))
            {
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
                Mathf.Abs(first.EndTiming - second.Timing) <= 1
             && first.XEnd == second.XStart
             && first.YEnd == second.YStart
             && first.IsTrace == second.IsTrace;
        }
    }
}