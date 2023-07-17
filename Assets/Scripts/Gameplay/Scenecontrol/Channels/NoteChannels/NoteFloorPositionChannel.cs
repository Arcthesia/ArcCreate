using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class NoteFloorPositionChannel : ValueChannel
    {
        public NoteFloorPositionChannel()
        {
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>();
        }

        public override float ValueAt(int timing)
            => (float?)NoteIndividualController.CurrentNote?.FloorPosition ?? 0;

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield break;
        }
    }
}