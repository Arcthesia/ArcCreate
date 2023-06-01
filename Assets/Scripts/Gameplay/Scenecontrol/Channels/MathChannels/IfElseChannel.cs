using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class IfElseChannel : ValueChannel
    {
        private BooleanChannel cond;
        private ValueChannel onTrue;
        private ValueChannel onFalse;

        public IfElseChannel()
        {
        }

        public IfElseChannel(BooleanChannel cond, ValueChannel onTrue, ValueChannel onFalse)
        {
            this.cond = cond;
            this.onTrue = onTrue;
            this.onFalse = onFalse;
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            cond = deserialization.GetUnitFromId<BooleanChannel>(properties[0]);
            onTrue = deserialization.GetUnitFromId<ValueChannel>(properties[1]);
            onFalse = deserialization.GetUnitFromId<ValueChannel>(properties[2]);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(cond),
                serialization.AddUnitAndGetId(onTrue),
                serialization.AddUnitAndGetId(onFalse),
            };
        }

        public override float ValueAt(int timing)
        {
            return cond.ValueAt(timing) ? onTrue.ValueAt(timing) : onFalse.ValueAt(timing);
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield return onTrue;
        }
    }
}