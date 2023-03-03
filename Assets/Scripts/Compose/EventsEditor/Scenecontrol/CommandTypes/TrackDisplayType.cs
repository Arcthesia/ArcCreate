using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class TrackDisplayType : IBuiltInScenecontrolType
    {
        public string Typename => "trackdisplay";

        public string[] ArgumentNames => new string[] { "duration", "alpha" };

        public void ExecuteCommand(ScenecontrolEvent ev)
        {
            int timing = ev.Timing;
            int duration = Mathf.RoundToInt((float)ev.Arguments[0]);
            float alpha = (float)ev.Arguments[1];
            float darkenAlpha = Mathf.Approximately(alpha, 255f) ? 0 : 255;
            ScDataSource.TrackAlphaFactor.AddKey(timing, ScDataSource.TrackAlphaFactor.ValueAt(timing));
            ScDataSource.TrackAlphaFactor.AddKey(timing + duration, alpha / 255f);
            ScDataSource.DarkenAlphaFactor.AddKey(timing, ScDataSource.DarkenAlphaFactor.ValueAt(timing));
            ScDataSource.DarkenAlphaFactor.AddKey(timing + 250, darkenAlpha / 255f);
        }
    }
}