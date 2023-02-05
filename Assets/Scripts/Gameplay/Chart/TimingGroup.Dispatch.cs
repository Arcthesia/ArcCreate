using System.Collections.Generic;
using System.Linq;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    /// <summary>
    /// Contains methods for CRUD operations.
    /// </summary>
    public partial class TimingGroup
    {
        private TapNoteGroup taps;
        private HoldNoteGroup holds;
        private ArcNoteGroup arcs;
        private ArcTapNoteGroup arcTaps;
        private GroupProperties groupProperties;
        private Transform parent;

        public TimingGroup(int tg)
        {
            GroupNumber = tg;
        }

        public int GroupNumber { get; private set; }

        public GroupProperties GroupProperties => groupProperties;

        // TODO: logic for scenecontrol to set visibility at the same time
        public bool IsVisible
        {
            get => parent.gameObject.activeSelf;
            set => parent.gameObject.SetActive(value);
        }

        /// <summary>
        /// Load a timing group data representation into this instance.
        /// </summary>
        /// <param name="tg">The timing group data.</param>
        /// <param name="parent">The parent transform for all notes of this timing group.</param>
        public void Load(ChartTimingGroup tg, Transform parent)
        {
            this.parent = parent;
            groupProperties = new GroupProperties(tg.Properties);

            taps = new TapNoteGroup();
            holds = new HoldNoteGroup();
            arcs = new ArcNoteGroup();
            arcTaps = new ArcTapNoteGroup();

            taps.Load(tg.Taps, parent);
            holds.Load(tg.Holds, parent);
            arcs.Load(tg.Arcs, parent);
            arcTaps.Load(tg.ArcTaps, parent);
            timings = tg.Timings;
            taps.SetupNotes();
            holds.SetupNotes();
            arcs.SetupNotes();
            arcTaps.SetupNotes();
            OnTimingListChange();
        }

        /// <summary>
        /// Load an empty timing group.
        /// </summary>
        /// <param name="parent">The parent transform for all notes of this timing group.</param>
        public void Load(Transform parent)
        {
            taps = new TapNoteGroup();
            holds = new HoldNoteGroup();
            arcs = new ArcNoteGroup();
            arcTaps = new ArcTapNoteGroup();

            this.parent = parent;
            groupProperties = new GroupProperties();
            timings = new List<TimingEvent>
            {
                new TimingEvent
                {
                    TimingGroup = GroupNumber,
                    Timing = 0,
                    Bpm = Values.BaseBpm,
                    Divisor = 4f,
                },
            };
            taps.Load(new List<Tap>(), parent);
            holds.Load(new List<Hold>(), parent);
            arcs.Load(new List<Arc>(), parent);
            arcTaps.Load(new List<ArcTap>(), parent);
        }

        public void SetGroupProperties(GroupProperties prop)
        {
            groupProperties = prop;
        }

        /// <summary>
        /// Update the group.
        /// </summary>
        /// <param name="timing">The timing to update the group to.</param>
        public void UpdateGroup(int timing)
        {
            double floorPosition = GetFloorPosition(timing);
            taps.Update(timing, floorPosition, groupProperties);
            holds.Update(timing, floorPosition, groupProperties);
            arcs.Update(timing, floorPosition, groupProperties);
            arcTaps.Update(timing, floorPosition, groupProperties);
        }

        /// <summary>
        /// Reload the skin of all notes of this timing group.
        /// </summary>
        public void ReloadSkin()
        {
            taps.ReloadSkin();
            holds.ReloadSkin();
            arcs.ReloadSkin();
            arcTaps.ReloadSkin();
        }

        /// <summary>
        /// Reset the judge of all notes of this timing group.
        /// </summary>
        public void ResetJudge()
        {
            taps.ResetJudge();
            holds.ResetJudge();
            arcs.ResetJudge();
            arcTaps.ResetJudge();
        }

        /// <summary>
        /// Get the total max combo count at provided timing value.
        /// </summary>
        /// <param name="timing">The timing value.</param>
        /// <returns>Max combo at the specified timing.</returns>
        public int ComboAt(int timing)
        {
            if (groupProperties.NoInput)
            {
                return 0;
            }

            int combo = 0;
            combo += taps.ComboAt(timing);
            combo += holds.ComboAt(timing);
            combo += arcs.ComboAt(timing);
            combo += arcTaps.ComboAt(timing);
            return combo;
        }

        /// <summary>
        /// Total combo count of all notes of this timing group.
        /// </summary>
        /// <returns>The total combo count.</returns>
        public int TotalCombo()
        {
            if (groupProperties.NoInput)
            {
                return 0;
            }

            int combo = 0;
            combo += taps.TotalCombo();
            combo += holds.TotalCombo();
            combo += arcs.TotalCombo();
            combo += arcTaps.TotalCombo();
            return combo;
        }

        /// <summary>
        /// Gets all events of type <c>T</c>.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <returns>List of events of the type <c>T</c>.</returns>
        public IEnumerable<T> GetEventType<T>()
            where T : ArcEvent
        {
            if (typeof(T) == typeof(Tap))
            {
                return taps.Notes.Cast<T>();
            }

            if (typeof(T) == typeof(Hold))
            {
                return holds.Notes.Cast<T>();
            }

            if (typeof(T) == typeof(ArcTap))
            {
                return arcTaps.Notes.Cast<T>();
            }

            if (typeof(T) == typeof(Arc))
            {
                return arcs.Notes.Cast<T>();
            }

            if (typeof(T) == typeof(TimingEvent))
            {
                return timings.Cast<T>();
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Find all events of this group that have matching timing value.
        /// </summary>
        /// <param name="timing">The query timing value.</param>
        /// <typeparam name="T">Event type to search for.</typeparam>
        /// <returns>All events with matching timing value.</returns>
        public IEnumerable<T> FindByTiming<T>(int timing)
            where T : ArcEvent
        {
            if (typeof(T) == typeof(Tap))
            {
                return taps.FindByTiming(timing).Cast<T>();
            }

            if (typeof(T) == typeof(Hold))
            {
                return holds.FindByTiming(timing).Cast<T>();
            }

            if (typeof(T) == typeof(ArcTap))
            {
                return arcTaps.FindByTiming(timing).Cast<T>();
            }

            if (typeof(T) == typeof(Arc))
            {
                return arcs.FindByTiming(timing).Cast<T>();
            }

            if (typeof(T) == typeof(TimingEvent))
            {
                return FindTimingEventsByTiming(timing).Cast<T>();
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Find all long notes of this group that have matching end timing value.
        /// </summary>
        /// <param name="endTiming">The query end timing value.</param>
        /// <typeparam name="T">Long note type to search for.</typeparam>
        /// <returns>All long notes with matching end timing value.</returns>
        public IEnumerable<T> FindByEndTiming<T>(int endTiming)
            where T : LongNote
        {
            if (typeof(T) == typeof(Hold))
            {
                return holds.FindByEndTiming(endTiming).Cast<T>();
            }

            if (typeof(T) == typeof(Arc))
            {
                return arcs.FindByEndTiming(endTiming).Cast<T>();
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Find all events of this group that are bounded by the provided timing range.
        /// </summary>
        /// <param name="from">The query timing lower range.</param>
        /// <param name="to">The query timing upper range.</param>
        /// <typeparam name="T">Event type to search for.</typeparam>
        /// <returns>All events with matching timing value.</returns>
        public IEnumerable<T> FindEventsWithinRange<T>(int from, int to)
            where T : ArcEvent
        {
            if (typeof(T) == typeof(Tap))
            {
                return taps.FindEventsWithinRange(from, to).Cast<T>();
            }

            if (typeof(T) == typeof(Hold))
            {
                return holds.FindEventsWithinRange(from, to).Cast<T>();
            }

            if (typeof(T) == typeof(ArcTap))
            {
                return arcTaps.FindEventsWithinRange(from, to).Cast<T>();
            }

            if (typeof(T) == typeof(Arc))
            {
                return arcs.FindEventsWithinRange(from, to).Cast<T>();
            }

            if (typeof(T) == typeof(TimingEvent))
            {
                return FindTimingEventsWithinRange(from, to).Cast<T>();
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Clear notes from this timing gruop and destroy all notes.
        /// </summary>
        public void Clear()
        {
            Object.Destroy(parent.gameObject);
            taps.Clear();
            holds.Clear();
            arcTaps.Clear();
            arcs.Clear();
        }

        public void CleanArcColliders()
        {
            foreach (var note in arcs.Notes)
            {
                note.CleanColliderMesh();
            }
        }

        public void BuildArcColliders()
        {
            foreach (var note in arcs.Notes)
            {
                note.Rebuild();
            }
        }

        /// <summary>
        /// Add a collection of events to this timing group.
        /// </summary>
        /// <param name="ev">The event collection.</param>
        public void AddEvents(IEnumerable<ArcEvent> ev)
        {
            (IEnumerable<Tap> taps,
            IEnumerable<Hold> holds,
            IEnumerable<Arc> arcs,
            IEnumerable<ArcTap> arcTaps,
            IEnumerable<TimingEvent> timings) = SplitInput(ev);

            if (taps.Any())
            {
                this.taps.Add(taps);
            }

            if (holds.Any())
            {
                this.holds.Add(holds);
            }

            if (arcs.Any())
            {
                this.arcs.Add(arcs);
            }

            if (arcTaps.Any())
            {
                this.arcTaps.Add(arcTaps);
            }

            if (timings.Any())
            {
                AddTimings(timings);
            }
        }

        /// <summary>
        /// Remove a collection of events from this timing group.
        /// </summary>
        /// <param name="ev">The event collection.</param>
        public void RemoveEvents(IEnumerable<ArcEvent> ev)
        {
            (IEnumerable<Tap> taps,
            IEnumerable<Hold> holds,
            IEnumerable<Arc> arcs,
            IEnumerable<ArcTap> arcTaps,
            IEnumerable<TimingEvent> timings) = SplitInput(ev);

            if (taps.Any())
            {
                this.taps.Remove(taps);
            }

            if (holds.Any())
            {
                this.holds.Remove(holds);
            }

            if (arcs.Any())
            {
                this.arcs.Remove(arcs);
            }

            if (arcTaps.Any())
            {
                this.arcTaps.Remove(arcTaps);
            }

            if (timings.Any())
            {
                RemoveTimings(timings);
            }
        }

        /// <summary>
        /// Notify that a collection of events have had their properties changed.
        /// </summary>
        /// <param name="ev">The event collection.</param>
        public void UpdateEvents(IEnumerable<ArcEvent> ev)
        {
            (IEnumerable<Tap> taps,
            IEnumerable<Hold> holds,
            IEnumerable<Arc> arcs,
            IEnumerable<ArcTap> arcTaps,
            IEnumerable<TimingEvent> timings) = SplitInput(ev);

            if (taps.Any())
            {
                this.taps.Update(taps);
            }

            if (holds.Any())
            {
                this.holds.Update(holds);
            }

            if (arcs.Any())
            {
                this.arcs.Update(arcs);
            }

            if (arcTaps.Any())
            {
                this.arcTaps.Update(arcTaps);
            }

            if (timings.Any())
            {
                UpdateTimings();
            }
        }

        /// <summary>
        /// Set the timing group's properties.
        /// </summary>
        /// <param name="prop">The properties object.</param>
        public void SetProperties(RawTimingGroup prop)
        {
            groupProperties = new GroupProperties(prop);
        }

        private (
            IEnumerable<Tap> taps,
            IEnumerable<Hold> holds,
            IEnumerable<Arc> arcs,
            IEnumerable<ArcTap> arcTaps,
            IEnumerable<TimingEvent> timings)
            SplitInput(IEnumerable<ArcEvent> e)
        {
            IEnumerable<Tap> taps = e.Where(n => n is Tap).Cast<Tap>();
            IEnumerable<Hold> holds = e.Where(n => n is Hold).Cast<Hold>();
            IEnumerable<Arc> arcs = e.Where(n => n is Arc).Cast<Arc>();
            IEnumerable<ArcTap> arcTaps = e.Where(n => n is ArcTap).Cast<ArcTap>();
            IEnumerable<TimingEvent> timings = e.Where(n => n is TimingEvent).Cast<TimingEvent>();
            return (taps, holds, arcs, arcTaps, timings);
        }
    }
}