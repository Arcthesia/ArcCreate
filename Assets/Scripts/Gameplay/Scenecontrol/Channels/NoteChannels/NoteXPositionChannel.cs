using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class NoteXPositionChannel : ValueChannel
    {
        public NoteXPositionChannel()
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

            return ArcFormula.WorldPosition(NoteIndividualController.CurrentNote).x;
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield break;
        }
    }
}