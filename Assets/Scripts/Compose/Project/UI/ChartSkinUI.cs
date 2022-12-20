using System;
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
            side.SetValueWithoutNotify(chart.Skin.Side);
            note.SetValueWithoutNotify(chart.Skin.Note);
            particle.SetValueWithoutNotify(chart.Skin.Particle);
            accent.SetValueWithoutNotify(chart.Skin.Accent);
            track.SetValueWithoutNotify(chart.Skin.Track);
            singleLine.SetValueWithoutNotify(chart.Skin.SingleLine);
        }

        private new void Awake()
        {
            base.Awake();
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

        private void OnSide(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnNote(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnParticle(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnAccent(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnTrack(string obj)
        {
            throw new NotImplementedException();
        }

        private void OnSingleLine(string obj)
        {
            throw new NotImplementedException();
        }
    }
}