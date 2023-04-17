using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Scenecontrol;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class EnwidenCameraType : IBuiltInScenecontrolType
    {
        private bool setup;
        private KeyChannel enwidenCameraFactor;

        public string Typename => "enwidencamera";

        public string[] ArgumentNames => new string[] { "duration", "toggle" };

        public void ExecuteCommand(ScenecontrolEvent ev)
        {
            if (!setup)
            {
                SetupData();
                setup = true;
            }

            int timing = ev.Timing;
            int duration = Mathf.RoundToInt((float)ev.Arguments[0]);
            float toggle = (float)ev.Arguments[1] >= 0.5f ? 1 : 0;

            enwidenCameraFactor.AddKey(timing, enwidenCameraFactor.ValueAt(timing));
            enwidenCameraFactor.AddKey(timing + duration, toggle);
        }

        private void SetupData()
        {
            enwidenCameraFactor = new KeyChannel().SetDefaultEasing("l").AddKey(-999999, 0);

            var camera = Services.Gameplay.Scenecontrol.Scene.GameplayCamera;
            var skyline = Services.Gameplay.Scenecontrol.Scene.SkyInputLine;
            var skylabel = Services.Gameplay.Scenecontrol.Scene.SkyInputLabel;
            var singleL = Services.Gameplay.Scenecontrol.Scene.SingleLineL;
            var singleR = Services.Gameplay.Scenecontrol.Scene.SingleLineR;

            ValueChannel ypos = (Context.Is16By9 * 1.5f) + 3;
            float skyDeltaY = 2.745f;
            float singleDeltaX = 5;
            camera.TranslationY += enwidenCameraFactor * 4.5f;
            camera.TranslationZ += enwidenCameraFactor * ypos;
            skyline.TranslationY += enwidenCameraFactor * skyDeltaY;
            skylabel.TranslationY += enwidenCameraFactor * skyDeltaY;
            singleL.TranslationX += enwidenCameraFactor * singleDeltaX;
            singleR.TranslationX -= enwidenCameraFactor * singleDeltaX;
        }
    }
}