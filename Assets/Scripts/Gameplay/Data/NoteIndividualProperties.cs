using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Skin;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class NoteIndividualProperties
    {
        private Dictionary<Note, NoteProperties> properties;

        public bool IsEnabled => properties != null;

        public bool UseColor { get; set; } = false;

        public void Enable()
        {
            properties = new Dictionary<Note, NoteProperties>();
        }

        public void Disable()
        {
            properties = null;
        }

        public void Clear()
        {
            properties.Clear();
        }

        /// <summary>
        /// Get the properties for a given note, which can be modified
        /// for use later on. Returns `null` if <see cref="IsEnabled"/>
        /// returns false.
        /// </summary>
        /// <returns>The properties which are associated with a note.</returns>
        /// <param name="note">The note to find the properties of.</param>
        public NoteProperties PropertiesFor(Note note)
        {
            if (!IsEnabled)
            {
                return null;
            }

            if (!properties.TryGetValue(note, out var prop))
            {
                prop = new NoteProperties();
                properties[note] = prop;
            }

            return prop;
        }
    }
}