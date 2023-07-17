using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class NoteIsArcChannel : BooleanChannel
    {
        public NoteIsArcChannel()
        {
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>();
        }

        public override bool ValueAt(int timing)
            => NoteIndividualController.CurrentNote is Arc;
    }
}