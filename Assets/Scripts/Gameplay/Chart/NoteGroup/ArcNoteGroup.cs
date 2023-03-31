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
                    if (arc.PreviousArc == null
                     || arc.PreviousArc.Color == overlap.Color)
                    {
                        arc.PreviousArc = overlap;
                    }

                    if (overlap.NextArc == null
                     || overlap.NextArc.Color == arc.Color)
                    {
                        overlap.NextArc = arc;
                    }
                }
            }

            foreach (Arc overlap in Services.Chart.FindByTiming<Arc>(arc.EndTiming - 1, arc.EndTiming + 1))
            {
                if (IsChained(arc, overlap))
                {
                    if (arc.NextArc == null
                     || arc.NextArc.Color == overlap.Color)
                    {
                        arc.NextArc = overlap;
                    }

                    if (overlap.PreviousArc == null
                     || overlap.PreviousArc.Color == arc.Color)
                    {
                        overlap.PreviousArc = arc;
                    }
                }
            }
        }

        private void RemoveArcFromChainGroups(Arc arc)
        {
            if (arc.NextArc != null && arc.NextArc.PreviousArc == arc)
            {
                arc.NextArc.PreviousArc = null;
            }

            if (arc.PreviousArc != null && arc.PreviousArc.NextArc == arc)
            {
                arc.PreviousArc.NextArc = null;
            }

            arc.NextArc = null;
            arc.PreviousArc = null;
        }

        private bool IsChained(Arc first, Arc second)
        {
            return
                !ReferenceEquals(first, second)
             && Mathf.Abs(first.EndTiming - second.Timing) <= 1
             && first.XEnd == second.XStart
             && first.YEnd == second.YStart
             && first.IsTrace == second.IsTrace;
        }
    }
}