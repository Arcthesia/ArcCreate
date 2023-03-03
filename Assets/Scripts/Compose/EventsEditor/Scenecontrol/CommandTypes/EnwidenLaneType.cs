using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class EnwidenLaneType : IBuiltInScenecontrolType
    {
        public string Typename => "enwidenlane";

        public string[] ArgumentNames => new string[] { "duration", "toggle" };

        public void ExecuteCommand(ScenecontrolEvent ev)
        {
            int timing = ev.Timing;
            int duration = Mathf.RoundToInt((float)ev.Arguments[0]);
            float toggle = (float)ev.Arguments[1] >= 0.5f ? 1 : 0;

            ScDataSource.EnwidenLaneFactor.AddKey(timing, ScDataSource.EnwidenLaneFactor.ValueAt(timing));
            ScDataSource.EnwidenLaneFactor.AddKey(timing + duration, toggle);
        }
    }
}