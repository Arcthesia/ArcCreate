using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Chart
{
    /// <summary>
    /// Handler for a group of notes of the same type, and belonging to the same timing group.
    /// </summary>
    /// <typeparam name="Note">The note type.</typeparam>
    /// <typeparam name="Behaviour">The MonoBehaviour component type corresponding to the note type.</typeparam>
    public abstract class NoteGroup<Note, Behaviour>
        where Note : INote<Behaviour>
        where Behaviour : MonoBehaviour
    {
        private List<Note> notes = new List<Note>();

        public List<Note> Notes => notes;

        public abstract string PoolName { get; }

        protected Transform ParentTransform { get; private set; }

        protected Pool<Behaviour> Pool { get; private set; }

        /// <summary>
        /// Clear the note group.
        /// </summary>
        public virtual void Clear()
        {
            Pool.ReturnAll();
            notes.Clear();
        }

        /// <summary>
        /// Load notes into this note group.
        /// </summary>
        /// <param name="notes">The notes to load.</param>
        /// <param name="parent">The parent transform of the notes.</param>
        public void Load(List<Note> notes, Transform parent)
        {
            Pool = Pools.Get<Behaviour>(PoolName);
            ParentTransform = parent;
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
                if (note.IsAssignedInstance)
                {
                    note.ReloadSkin();
                }
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
            RebuildList();
            foreach (var note in notes)
            {
                OnAdd(note);
                note.Rebuild();
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
                if (note.IsAssignedInstance)
                {
                    Pool.Return(note.RevokeInstance());
                }
            }
        }

        /// <summary>
        /// Notify that a collection of notes from this group have had their properties changed.
        /// </summary>
        /// <param name="notes">The note collection.</param>
        public void Update(IEnumerable<Note> notes)
        {
            RebuildList();
            foreach (var note in notes)
            {
                OnUpdate(note);
                note.Rebuild();
            }
        }

        /// <summary>
        /// Update the state of all notes of this group to the new timing value.
        /// </summary>
        /// <param name="timing">The timing value.</param>
        /// <param name="floorPosition">Floor position value corresponding to the timing value.</param>
        /// <param name="groupProperties">The group properties of the notes' timing group.</param>
        public abstract void Update(int timing, double floorPosition, GroupProperties groupProperties);

        /// <summary>
        /// Called every time there's a change to the note list.
        /// </summary>
        public abstract void RebuildList();

        /// <summary>
        /// Find all notes of this group that match the queried timing.
        /// </summary>
        /// <param name="timing">The query timing value.</param>
        /// <returns>All matching notes of this note group.</returns>
        public abstract IEnumerable<Note> FindByTiming(int timing);

        /// <summary>
        /// Find all notes of this group that are bounded by the provided timing range.
        /// </summary>
        /// <param name="from">The query timing lower range.</param>
        /// <param name="to">The query timing upper range.</param>
        /// <returns>All notes with matching timing value.</returns>
        public abstract IEnumerable<Note> FindEventsWithinRange(int from, int to);

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