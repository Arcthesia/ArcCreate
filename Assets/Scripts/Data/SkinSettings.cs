using System;

namespace ArcCreate.Data
{
    public class SkinSettings
    {
        public string Side { get; set; } = null;

        public string Note { get; set; } = null;

        public string Particle { get; set; } = null;

        public string Track { get; set; } = null;

        public string Accent { get; set; } = null;

        public string SingleLine { get; set; } = null;

        public SkinSettings Clone()
        {
            return new SkinSettings
            {
                Side = Side,
                Note = Note,
                Particle = Particle,
                Track = Track,
                Accent = Accent,
                SingleLine = SingleLine,
            };
        }
    }
}