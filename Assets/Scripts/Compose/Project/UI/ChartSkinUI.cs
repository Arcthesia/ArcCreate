using System;
using ArcCreate.Compose.Components;
using ArcCreate.Data;
using ArcCreate.Utility;
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
        [SerializeField] private ThemeGroup themeGroup;

        protected override void ApplyChartSettings(ChartSettings chart)
        {
            side.SetValueWithoutNotify(chart.Skin?.Side);
            note.SetValueWithoutNotify(chart.Skin?.Note);
            particle.SetValueWithoutNotify(chart.Skin?.Particle);
            accent.SetValueWithoutNotify(chart.Skin?.Accent);
            track.SetValueWithoutNotify(chart.Skin?.Track);
            singleLine.SetValueWithoutNotify(chart.Skin?.SingleLine);

            ApplySide(chart.Skin?.Side);
            Services.Gameplay.Skin.NoteSkin = chart.Skin?.Note;
            Services.Gameplay.Skin.ParticleSkin = chart.Skin?.Particle;
            Services.Gameplay.Skin.AccentSkin = chart.Skin?.Accent;
            Services.Gameplay.Skin.TrackSkin = chart.Skin?.Track;
            Services.Gameplay.Skin.SingleLineSkin = chart.Skin?.SingleLine;
        }

        private new void Start()
        {
            base.Start();
            side.OnSelect += OnSide;
            note.OnSelect += OnNote;
            particle.OnSelect += OnParticle;
            accent.OnSelect += OnAccent;
            track.OnSelect += OnTrack;
            singleLine.OnSelect += OnSingleLine;
        }

        private void OnDestroy()
        {
            side.OnSelect -= OnSide;
            note.OnSelect -= OnNote;
            particle.OnSelect -= OnParticle;
            accent.OnSelect -= OnAccent;
            track.OnSelect -= OnTrack;
            singleLine.OnSelect -= OnSingleLine;
        }

        private void OnSide(string value)
        {
            CreateSkinObjectIfNull();
            Target.Skin.Side = value;
            ApplySide(value);

            Values.ProjectModified = true;
        }

        private void ApplySide(string value)
        {
            Services.Gameplay.Skin.AlignmentSkin = value;
            switch (Services.Gameplay.Skin.AlignmentSkin)
            {
                case "conflict":
                    themeGroup.Value = Theme.Dark;
                    break;
                case "light":
                case "colorless":
                default:
                    themeGroup.Value = Theme.Light;
                    break;
            }
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