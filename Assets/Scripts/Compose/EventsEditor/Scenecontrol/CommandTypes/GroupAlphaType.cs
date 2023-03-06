using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Scenecontrol;
using UnityEngine;

namespace ArcCreate.Compose.EventsEditor
{
    public class GroupAlphaType : IBuiltInScenecontrolType
    {
        public string Typename => "groupalpha";

        public string[] ArgumentNames => new string[] { "duration", "alpha" };

        public void ExecuteCommand(ScenecontrolEvent ev)
        {
            int timing = ev.Timing;
            int duration = Mathf.RoundToInt((float)ev.Arguments[0]);
            float alpha = (float)ev.Arguments[1];

            NoteGroupController noteGroup = Services.Gameplay.Scenecontrol.Scene.GetNoteGroup(ev.TimingGroup);

            ValueChannel channel = noteGroup.ColorA.Find("internal");
            if (channel == null)
            {
                channel = new KeyChannel().SetDefaultEasing("l").AddKey(0, 1);
                channel.Name = "internal";
                noteGroup.ColorA *= channel;
            }

            KeyChannel c = channel as KeyChannel;

            c.AddKey(timing, c.ValueAt(timing));
            c.AddKey(timing + duration, alpha / 255f);
        }
    }
}