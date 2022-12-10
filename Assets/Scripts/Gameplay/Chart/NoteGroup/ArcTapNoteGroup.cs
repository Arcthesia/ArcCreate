using System.Collections.Generic;
using ArcCreate.Gameplay.Data;

namespace ArcCreate.Gameplay.Chart
{
    public class ArcTapNoteGroup : ShortNoteGroup<ArcTap, ArcTapBehaviour>
    {
        public override string PoolName => Values.ArcTapPoolName;

        protected override void OnAdd(ArcTap note)
        {
            SetupConnection(note);
        }

        protected override void OnUpdate(ArcTap note)
        {
            SetupConnection(note);
        }

        protected override void OnRemove(ArcTap note)
        {
            RemoveConnection(note);
        }

        protected override void SetupNotes()
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                ArcTap arcTap = Notes[i];
                SetupConnection(arcTap);
            }
        }

        private void SetupConnection(ArcTap note)
        {
            RemoveConnection(note);

            IEnumerable<Tap> connectedTaps
                = Services.Chart.FindByTiming<Tap>(note.Timing);

            foreach (Tap tap in connectedTaps)
            {
                note.ConnectedTaps.Add(tap);
            }
        }

        private void RemoveConnection(ArcTap note)
        {
            foreach (Tap tap in note.ConnectedTaps)
            {
                tap.ConnectedArcTaps.Remove(note);
                tap.Rebuild();
            }

            note.ConnectedTaps.Clear();
        }
    }
}