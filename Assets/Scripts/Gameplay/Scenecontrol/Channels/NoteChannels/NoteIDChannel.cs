using System.Collections.Generic;
using System.Runtime.Serialization;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class NoteIDChannel : ValueChannel
    {
        private static ObjectIDGenerator idGenerator = new ObjectIDGenerator();

        public NoteIDChannel()
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
        {
            if (NoteIndividualController.CurrentNote == null)
            {
                return 0;
            }

            return idGenerator.GetId(NoteIndividualController.CurrentNote, out _);
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield break;
        }
    }
}