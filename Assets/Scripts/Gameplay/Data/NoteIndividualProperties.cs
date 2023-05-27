using System.Collections.Generic;
using ArcCreate.ChartFormat;
using ArcCreate.Gameplay.Skin;
using UnityEngine;

namespace ArcCreate.Gameplay.Data
{
    public class NoteIndividualProperties
    {
        private readonly Dictionary<Note, NoteProperties> properties = new Dictionary<Note, NoteProperties>();

        public bool HasPropertiesFor(Note note)
        {
            return properties.ContainsKey(note);
        }

        public NoteProperties PropertiesFor(Note note)
        {
            if (!properties.TryGetValue(note, out var prop))
            {
                prop = new NoteProperties();
                properties[note] = prop;
            }

            return prop;
        }
    }
}