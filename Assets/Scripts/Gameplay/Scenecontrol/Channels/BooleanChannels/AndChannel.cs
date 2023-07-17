using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class AndChannel : BooleanChannel
    {
        private BooleanChannel a;
        private BooleanChannel b;

        public AndChannel()
        {
        }

        public AndChannel(BooleanChannel a, BooleanChannel b)
        {
            this.a = a;
            this.b = b;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            a = deserialization.GetUnitFromId<BooleanChannel>(properties[0]);
            b = deserialization.GetUnitFromId<BooleanChannel>(properties[1]);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>()
            {
                serialization.AddUnitAndGetId(a),
                serialization.AddUnitAndGetId(b),
            };
        }

        public override bool ValueAt(int timing)
            => a.ValueAt(timing) && b.ValueAt(timing);
    }
}