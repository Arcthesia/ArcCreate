using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class StringComparisonChannel : BooleanChannel
    {
        private StringChannel a;
        private StringChannel b;
        private ComparisonType comparison;

        public StringComparisonChannel()
        {
        }

        public StringComparisonChannel(StringChannel a, StringChannel b, ComparisonType c)
        {
            this.a = a;
            this.b = b;
            comparison = c;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            a = deserialization.GetUnitFromId<StringChannel>(properties[0]);
            b = deserialization.GetUnitFromId<StringChannel>(properties[1]);
            comparison = (ComparisonType)properties[2];
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>()
            {
                serialization.AddUnitAndGetId(a),
                serialization.AddUnitAndGetId(b),
                comparison,
            };
        }

        public override bool ValueAt(int timing)
            => comparison.Compare(a.ValueAt(timing), b.ValueAt(timing));
    }
}