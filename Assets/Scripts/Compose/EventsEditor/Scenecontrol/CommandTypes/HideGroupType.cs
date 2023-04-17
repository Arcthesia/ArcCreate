using ArcCreate.Gameplay.Data;
using ArcCreate.Gameplay.Scenecontrol;

namespace ArcCreate.Compose.EventsEditor
{
    public class HideGroupType : IBuiltInScenecontrolType
    {
        public string Typename => "hidegroup";

        public string[] ArgumentNames => new string[] { "unused", "hide" };

        public void ExecuteCommand(ScenecontrolEvent ev)
        {
            int timing = ev.Timing;
            float hidden = (float)ev.Arguments[1];

            NoteGroupController noteGroup = Services.Gameplay.Scenecontrol.Scene.GetNoteGroup(ev.TimingGroup);

            ValueChannel channel = noteGroup.Active.Find("internal");
            if (channel == null)
            {
                channel = new KeyChannel().SetDefaultEasing("cnsti").AddKey(-999999, 1);
                channel.Name = "internal";
                noteGroup.Active *= channel;
            }

            KeyChannel c = channel as KeyChannel;

            c.AddKey(timing, 1 - hidden);
        }
    }
}