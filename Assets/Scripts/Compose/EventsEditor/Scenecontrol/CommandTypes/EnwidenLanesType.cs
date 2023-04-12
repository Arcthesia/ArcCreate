using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Scenecontrol;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class EnwidenLanesType : IBuiltInScenecontrolType
    {
        private bool setup;
        private KeyChannel enwidenLaneFactor;

        public string Typename => "enwidenlanes";

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

            enwidenLaneFactor.AddKey(timing, enwidenLaneFactor.ValueAt(timing));
            enwidenLaneFactor.AddKey(timing + duration, toggle);
        }

        private void SetupData()
        {
            enwidenLaneFactor = new KeyChannel().SetDefaultEasing("l").AddKey(0, 0);
            var beatline = Services.Gameplay.Scenecontrol.Scene.Beatlines;

            ValueChannel posY = -100 * (1 - enwidenLaneFactor);
            ValueChannel alpha = 1 - enwidenLaneFactor;

            var track = Services.Gameplay.Scenecontrol.Scene.Track;
            var context = Services.Gameplay.Scenecontrol.Context;
            track.ExtraL.Active = new ConstantChannel(1);
            track.ExtraR.Active = new ConstantChannel(1);
            track.CriticalLine0.Active = new ConstantChannel(1);
            track.CriticalLine5.Active = new ConstantChannel(1);
            track.DivideLine01.Active = new ConstantChannel(1);
            track.DivideLine45.Active = new ConstantChannel(1);
            track.EdgeExtraL.Active = new ConstantChannel(1);
            track.EdgeExtraR.Active = new ConstantChannel(1);

            track.EdgeExtraL.ColorA *= enwidenLaneFactor;
            track.ExtraL.ColorA *= enwidenLaneFactor;
            track.ExtraL.TranslationY += posY;
            track.CriticalLine0.ColorA *= enwidenLaneFactor;
            track.DivideLine01.ColorA *= enwidenLaneFactor;
            track.EdgeLAlpha *= alpha;
            track.EdgeRAlpha *= alpha;
            track.DivideLine45.ColorA *= enwidenLaneFactor;
            track.CriticalLine5.ColorA *= enwidenLaneFactor;
            track.ExtraR.TranslationY += posY;
            track.ExtraR.ColorA *= enwidenLaneFactor;
            track.EdgeExtraR.ColorA *= enwidenLaneFactor;

            context.LaneTo += enwidenLaneFactor;
            context.LaneFrom -= enwidenLaneFactor;

            beatline.ScaleX *= (enwidenLaneFactor * 0.5f) + 1;
        }
    }
}