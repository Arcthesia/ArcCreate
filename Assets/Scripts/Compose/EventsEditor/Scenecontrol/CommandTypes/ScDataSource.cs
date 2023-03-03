using ArcCreate.Gameplay.Scenecontrol;

namespace ArcCreate.Compose.EventsEditor
{
    public class ScDataSource
    {
        public static KeyChannel TrackAlphaFactor { get; private set; }

        public static KeyChannel DarkenAlphaFactor { get; private set; }

        public static KeyChannel EnwidenLaneFactor { get; private set; }

        public static KeyChannel EnwidenCameraFactor { get; private set; }

        public static void SetupChannels()
        {
            TrackAlphaFactor = new KeyChannel().SetDefaultEasing("l").AddKey(0, 1);
            DarkenAlphaFactor = new KeyChannel().SetDefaultEasing("l").AddKey(0, 0);
            EnwidenLaneFactor = new KeyChannel().SetDefaultEasing("l").AddKey(0, 0);
            EnwidenCameraFactor = new KeyChannel().SetDefaultEasing("l").AddKey(0, 0);

            var track = Services.Gameplay.Scenecontrol.Scene.Track;
            var beatline = Services.Gameplay.Scenecontrol.Scene.Beatlines;
            track.ColorA *= TrackAlphaFactor;
            foreach (var obj in track.GetComponentsInChildren<SpriteController>())
            {
                obj.ColorA *= TrackAlphaFactor;
            }

            var darken = Services.Gameplay.Scenecontrol.Scene.Darken;
            darken.Active = new ConstantChannel(1);
            darken.ColorA *= DarkenAlphaFactor;

            ValueChannel posY = -100 * (1 - EnwidenLaneFactor);
            ValueChannel alpha = 1 - EnwidenLaneFactor;

            track.ExtraL.Active = new ConstantChannel(1);
            track.ExtraR.Active = new ConstantChannel(1);
            track.CriticalLine0.Active = new ConstantChannel(1);
            track.CriticalLine5.Active = new ConstantChannel(1);
            track.DivideLine01.Active = new ConstantChannel(1);
            track.DivideLine45.Active = new ConstantChannel(1);
            track.EdgeExtraL.Active = new ConstantChannel(1);
            track.EdgeExtraR.Active = new ConstantChannel(1);

            track.EdgeExtraL.ColorA *= EnwidenLaneFactor;
            track.ExtraL.ColorA *= EnwidenLaneFactor;
            track.ExtraL.TranslationY += posY;
            track.CriticalLine0.ColorA *= EnwidenLaneFactor;
            track.DivideLine01.ColorA *= EnwidenLaneFactor;
            track.EdgeLAlpha *= alpha;
            track.EdgeRAlpha *= alpha;
            track.DivideLine45.ColorA *= EnwidenLaneFactor;
            track.CriticalLine5.ColorA *= EnwidenLaneFactor;
            track.ExtraR.TranslationY += posY;
            track.ExtraR.ColorA *= EnwidenLaneFactor;
            track.EdgeExtraR.ColorA *= EnwidenLaneFactor;
            beatline.ScaleX *= (EnwidenLaneFactor * 0.5f) + 1;

            var camera = Services.Gameplay.Scenecontrol.Scene.GameplayCamera;
            var skyline = Services.Gameplay.Scenecontrol.Scene.SkyInputLine;
            var skylabel = Services.Gameplay.Scenecontrol.Scene.SkyInputLabel;
            var singleL = Services.Gameplay.Scenecontrol.Scene.SingleLineL;
            var singleR = Services.Gameplay.Scenecontrol.Scene.SingleLineR;

            ValueChannel ypos = (Context.Is16By9 * 1.5f) + 3;
            float skyDeltaY = 2.745f;
            float singleDeltaX = 5;
            camera.TranslationY += EnwidenCameraFactor * 4.5f;
            camera.TranslationZ += EnwidenCameraFactor * ypos;
            skyline.TranslationY += EnwidenCameraFactor * skyDeltaY;
            skylabel.TranslationY += EnwidenCameraFactor * skyDeltaY;
            singleL.TranslationX += EnwidenCameraFactor * singleDeltaX;
            singleR.TranslationX -= EnwidenCameraFactor * singleDeltaX;
        }
    }
}