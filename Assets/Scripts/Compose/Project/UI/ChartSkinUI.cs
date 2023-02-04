using ArcCreate.Compose.Components;
using UnityEngine;

namespace ArcCreate.Compose.Project
{
    public class ChartSkinUI : ChartMetadataUI
    {
        [SerializeField] private OptionsPanel side;
        [SerializeField] private OptionsPanel note;
        [SerializeField] private OptionsPanel particle;
        [SerializeField] private OptionsPanel accent;
        [SerializeField] private OptionsPanel track;
        [SerializeField] private OptionsPanel singleLine;

        protected override void ApplyChartSettings(ChartSettings chart)
        {
            side.Value = chart.Skin?.Side;
            note.Value = chart.Skin?.Note;
            particle.Value = chart.Skin?.Particle;
            accent.Value = chart.Skin?.Accent;
            track.Value = chart.Skin?.Track;
            singleLine.Value = chart.Skin?.SingleLine;
        }

        private new void Start()
        {
            base.Start();
            side.OnValueChanged += OnSide;
            note.OnValueChanged += OnNote;
            particle.OnValueChanged += OnParticle;
            accent.OnValueChanged += OnAccent;
            track.OnValueChanged += OnTrack;
            singleLine.OnValueChanged += OnSingleLine;
        }

        private void OnDestroy()
        {
            side.OnValueChanged -= OnSide;
            note.OnValueChanged -= OnNote;
            particle.OnValueChanged -= OnParticle;
            accent.OnValueChanged -= OnAccent;
            track.OnValueChanged -= OnTrack;
            singleLine.OnValueChanged -= OnSingleLine;
        }

        private void OnSide(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Side = value;
            Services.Gameplay.Skin.AlignmentSkin = value;

            Values.ProjectModified = true;
        }

        private void OnNote(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Note = value;
            Services.Gameplay.Skin.NoteSkin = value;

            Values.ProjectModified = true;
        }

        private void OnParticle(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Particle = value;
            Services.Gameplay.Skin.ParticleSkin = value;

            Values.ProjectModified = true;
        }

        private void OnAccent(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Accent = value;
            Services.Gameplay.Skin.AccentSkin = value;

            Values.ProjectModified = true;
        }

        private void OnTrack(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Track = value;
            Services.Gameplay.Skin.TrackSkin = value;

            Values.ProjectModified = true;
        }

        private void OnSingleLine(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.SingleLine = value;
            Services.Gameplay.Skin.SingleLineSkin = value;

            Values.ProjectModified = true;
        }

        private void CreateSkinObjectIfNull()
        {
            Target.Skin = Target.Skin ?? new SkinSettings();
        }
    }
}