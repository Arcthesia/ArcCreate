using System;
using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Chart;
using ArcCreate.Gameplay.Skin;
using ArcCreate.Utility;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class NoteIndividualProperties
    {
        private Dictionary<Note, NoteProperties> properties;

        public bool IsEnabled => properties != null;

        public bool UseColor { get; set; } = false;

        public bool UsePosition { get; set; } = false;

        public bool UseAngle { get; set; } = false;

        public void Enable(TimingGroup timinggroup)
        {
            properties = new Dictionary<Note, NoteProperties>();

            foreach (var note in timinggroup.GetAllNotes())
            {
                properties[note] = new NoteProperties();
            }
        }

        public void Disable()
        {
            properties = null;
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

        public void SetAllColors(Color color)
        {
            foreach (var props in properties.Values)
            {
                props.Color = color;
            }
        }

        public void SetAllTransforms(TRS transform)
        {
            foreach (var props in properties.Values)
            {
                props.Transform = transform;
            }
        }

        public void SetAllAngles(Vector2 xy)
        {
            foreach (var props in properties.Values)
            {
                props.Angles = xy;
            }
        }
    }
}