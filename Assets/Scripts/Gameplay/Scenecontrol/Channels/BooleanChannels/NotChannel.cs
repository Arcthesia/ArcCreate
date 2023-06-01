using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class NotChannel : BooleanChannel
    {
        private BooleanChannel a;

        public NotChannel()
        {
        }

        public NotChannel(BooleanChannel a)
        {
            this.a = a;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            a = deserialization.GetUnitFromId<BooleanChannel>(properties[0]);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>()
            {
                serialization.AddUnitAndGetId(a),
            };
        }

        public override bool ValueAt(int timing)
            => !a.ValueAt(timing);
    }
}