using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    /// <summary>
    /// Handler for a group of notes of the same type, and belonging to the same timing group.
    /// </summary>
    /// <typeparam name="Note">The note type.</typeparam>
    public abstract class NoteGroup<Note>
        where Note : INote
    {
        private List<Note> notes = new List<Note>();

        public List<Note> Notes => notes;

        /// <summary>
        /// Clear the note group.
        /// </summary>
        public virtual void Clear()
        {
            notes.Clear();
        }

        /// <summary>
        /// Load notes into this note group.
        /// </summary>
        /// <param name="notes">The notes to load.</param>
        public void Load(List<Note> notes)
        {
            this.notes = notes;
            RebuildList();
        }

        /// <summary>
        /// Reload the skin of all notes of this note group.
        /// </summary>
        public void ReloadSkin()
        {
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];
                note.ReloadSkin();
            }
        }

        /// <summary>
        /// Reset judgement of all notes of this group.
        /// </summary>
        /// <param name="timing">The new timing to reset to.</param>
        public virtual void ResetJudgeTo(int timing)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];
                note.ResetJudgeTo(timing);
            }
        }

        /// <summary>
        /// Get the total max combo count at provided timing value.
        /// </summary>
        /// <param name="timing">The timing value.</param>
        /// <returns>Max combo at the specified timing.</returns>
        public abstract int ComboAt(int timing);

        /// <summary>
        /// Total combo count of all notes of this group.
        /// </summary>
        /// <returns>The combo count.</returns>
        public int TotalCombo()
        {
            int combo = 0;
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];
                combo += note.TotalCombo;
            }

            return combo;
        }

        /// <summary>
        /// Add a collection of notes to this group.
        /// </summary>
        /// <param name="notes">The note collection.</param>
        public void Add(IEnumerable<Note> notes)
        {
            this.notes.AddRange(notes);
            foreach (var note in notes)
            {
                note.RecalculateFloorPosition();
            }

            RebuildList();

            foreach (var note in notes)
            {
                OnAdd(note);
                note.Rebuild();
                note.ReloadSkin();
            }
        }

        /// <summary>
        /// Remove a collection of notes from this group.
        /// </summary>
        /// <param name="notes">The note collection.</param>
        public void Remove(IEnumerable<Note> notes)
        {
            foreach (var note in notes)
            {
                OnRemove(note);
                this.notes.Remove(note);
            }

            RebuildList();
        }

        /// <summary>
        /// Notify that a collection of notes from this group have had their properties changed.
        /// </summary>
        /// <param name="notes">The note collection.</param>
        public void Update(IEnumerable<Note> notes)
        {
            foreach (var note in notes)
            {
                note.RecalculateFloorPosition();
            }

            UpdateList();

            foreach (var note in notes)
            {
                OnUpdate(note);
                note.Rebuild();
                note.ReloadSkin();
            }
        }

        /// <summary>
        /// Update judgement state of all notes of this group to the new timing value.
        /// </summary>
        /// <param name="timing">The timing value.</param>
        /// <param name="floorPosition">Floor position value corresponding to the timing value.</param>
        /// <param name="groupProperties">The group properties of the notes' timing group.</param>
        public abstract void UpdateJudgement(int timing, double floorPosition, GroupProperties groupProperties);

        /// <summary>
        /// Update render state of all notes of this group to the new timing value.
        /// </summary>
        /// <param name="timing">The timing value.</param>
        /// <param name="floorPosition">Floor position value corresponding to the timing value.</param>
        /// <param name="groupProperties">The group properties of the notes' timing group.</param>
        public abstract void UpdateRender(int timing, double floorPosition, GroupProperties groupProperties);

        /// <summary>
        /// Called every time there's a change to the note list.
        /// </summary>
        public abstract void RebuildList();

        /// <summary>
        /// Called every time there's a change to note's value but the list's size stays unchanged.
        /// </summary>
        public abstract void UpdateList();

        /// <summary>
        /// Find all notes of this group that match the queried timing.
        /// </summary>
        /// <param name="from">The query timing value range's lower end.</param>
        /// <param name="to">The query timing value range's upper end.</param>
        /// <returns>All matching notes of this note group.</returns>
        public abstract IEnumerable<Note> FindByTiming(int from, int to);

        /// <summary>
        /// Find all notes of this group that are bounded by the provided timing range.
        /// </summary>
        /// <param name="from">The query timing lower range.</param>
        /// <param name="to">The query timing upper range.</param>
        /// <param name="overlapCompletely">Whether to only query for notes that overlap with the range completely.</param>
        /// <returns>All notes with matching timing value.</returns>
        public abstract IEnumerable<Note> FindEventsWithinRange(int from, int to, bool overlapCompletely);

        /// <summary>
        /// Find all rendering notes.
        /// </summary>
        /// <returns>List of rendering notes.</returns>
        public abstract IEnumerable<Note> GetRenderingNotes();

        /// <summary>
        /// Called after notes are loaded into the group.
        /// </summary>
        public abstract void SetupNotes();

        /// <summary>
        /// Called after a note was added to the group.
        /// </summary>
        /// <param name="note">The added note.</param>
        protected abstract void OnAdd(Note note);

        /// <summary>
        /// Called after a note was removed from the group.
        /// </summary>
        /// <param name="note">The removed note.</param>
        protected abstract void OnRemove(Note note);

        /// <summary>
        /// Called after a note has had their properties changed.
        /// </summary>
        /// <param name="note">The changed note.</param>
        protected abstract void OnUpdate(Note note);
    }
}