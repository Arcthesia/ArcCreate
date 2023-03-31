using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Scenecontrol;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class TrackDisplayType : IBuiltInScenecontrolType
    {
        private bool setup;
        private KeyChannel trackAlphaFactor;
        private KeyChannel darkenAlphaFactor;

        public string Typename => "trackdisplay";

        public string[] ArgumentNames => new string[] { "duration", "alpha" };

        public void ExecuteCommand(ScenecontrolEvent ev)
        {
            if (!setup)
            {
                SetupData();
                setup = true;
            }

            int timing = ev.Timing;
            int duration = Mathf.RoundToInt((float)ev.Arguments[0]);
            float alpha = (float)ev.Arguments[1];
            float darkenAlpha = Mathf.Approximately(alpha, 255f) ? 0 : 255;
            trackAlphaFactor.AddKey(timing, trackAlphaFactor.ValueAt(timing));
            trackAlphaFactor.AddKey(timing + duration, alpha / 255f);
            darkenAlphaFactor.AddKey(timing, darkenAlphaFactor.ValueAt(timing));
            darkenAlphaFactor.AddKey(timing + 250, darkenAlpha / 255f);
        }

        private void SetupData()
        {
            trackAlphaFactor = new KeyChannel().SetDefaultEasing("l").AddKey(0, 1);
            darkenAlphaFactor = new KeyChannel().SetDefaultEasing("l").AddKey(0, 0);

            var track = Services.Gameplay.Scenecontrol.Scene.Track;
            track.ColorA *= trackAlphaFactor;
            track.ExtraL.ColorA *= trackAlphaFactor;
            track.ExtraR.ColorA *= trackAlphaFactor;
            track.EdgeExtraL.ColorA *= trackAlphaFactor;
            track.EdgeExtraR.ColorA *= trackAlphaFactor;

            foreach (var obj in track.GetComponentsInChildren<SpriteController>())
            {
                obj.ColorA *= trackAlphaFactor;
            }

            var darken = Services.Gameplay.Scenecontrol.Scene.Darken;
            darken.Active = new ConstantChannel(1);
            darken.ColorA *= darkenAlphaFactor;
        }
    }
}