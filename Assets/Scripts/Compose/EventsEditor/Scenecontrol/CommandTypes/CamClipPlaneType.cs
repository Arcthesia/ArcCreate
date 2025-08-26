using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Scenecontrol;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class CamClipPlaneType : IBuiltInScenecontrolType
    { 
        private bool setup;
        private KeyChannel nearChannel;
        private KeyChannel farChannel;

        public string Typename => "camclipplane";

        public string[] ArgumentNames => new string[] { "duration", "near", "far" };

        public void ExecuteCommand(ScenecontrolEvent ev)
        {
            if (!setup)
            {
                SetupData();
                setup = true;
            }

            int timing = ev.Timing;
            int duration = Mathf.RoundToInt((float)ev.Arguments[0]);
            float near = (float)ev.Arguments[1];
            float far = (float)ev.Arguments[2];

            nearChannel.AddKey(timing, nearChannel.ValueAt(timing));
            nearChannel.AddKey(timing + duration, near);
            farChannel.AddKey(timing, farChannel.ValueAt(timing));
            farChannel.AddKey(timing + duration, far);
        }

        private void SetupData()
        {
            nearChannel = new KeyChannel().SetDefaultEasing("l").AddKey(-999999, 0.01f);
            farChannel = new KeyChannel().SetDefaultEasing("l").AddKey(-999999, 10000f);

            var camera = Services.Gameplay.Scenecontrol.Scene.GameplayCamera;
            camera.Near = nearChannel;
            camera.Far = farChannel;
        }
    }
}