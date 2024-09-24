using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ObserveTrigger : Trigger
    {
        private ValueChannel target;
        private ValueChannel above;
        private ValueChannel below;

        private float lastDiffToAbove = float.MaxValue;
        private float lastDiffToBelow = float.MinValue;

        public ObserveTrigger()
        {
        }

        public ObserveTrigger(ValueChannel target)
        {
            this.target = target;
            above = new ConstantChannel(float.MaxValue);
            below = new ConstantChannel(float.MinValue);
        }

        [EmmyDoc("Set the upper threshold value. The channel activates if the target channel's value go above the lower threshold")]
        public ObserveTrigger GoAbove(ValueChannel above)
        {
            this.above = above;
            return this;
        }

        [EmmyDoc("Set the lower threshold value. The channel activates if the target channel's value go below the lower threshold")]
        public ObserveTrigger GoBelow(ValueChannel below)
        {
            this.below = below;
            return this;
        }

        [EmmyDoc("Sets the value to send to the TriggerChannel bound to this trigger.")]
        public ObserveTrigger Dispatch(
            ValueChannel value,
            ValueChannel duration = null,
#pragma warning disable
            [EmmyChoice(
                "linear", "l", "inconstant", "inconst", "cnsti",
                "outconstant", "outconst", "cnsto", "inoutconstant", "inoutconst",
                "cnstb", "insine", "si", "outsine", "so",
                "inoutsine", "b", "inquadratic", "inquad", "2i",
                "outquadratic", "outquad", "2o", "inoutquadratic", "inoutquad",
                "2b", "incubic", "3i", "outcubic", "outcube",
                "3o", "inoutcubic", "inoutcube", "3b", "inquartic",
                "inquart", "4i", "outquartic", "outquart", "4o",
                "inoutquartic", "inoutquart", "4b", "inquintic", "inquint",
                "5i", "outquintic", "outquint", "5o", "inoutquintic",
                "inoutquint", "5b", "inexponential", "inexpo", "exi",
                "outexponential", "outexpo", "exo", "inoutexponential", "inoutexpo",
                "exb", "incircle", "incirc", "ci", "outcircle",
                "outcirc", "co", "inoutcircle", "inoutcirc", "cb",
                "inback", "bki", "outback", "bko", "inoutback",
                "bkb", "inelastic", "eli", "outelastic", "elo",
                "inoutelastic", "elb", "inbounce", "bni", "outbounce",
                "bno", "inoutbounce", "bnb")]
            string easing = null)
#pragma warning restore
        {
            TriggerDispatch = new TriggerValueDispatch
            {
                Value = value,
                Duration = duration ?? ValueChannel.ConstantOneChannel,
                Easing = Easing.FromString(easing),
                EasingString = easing,
            };

            return this;
        }

        public override void Poll(int timing)
        {
            float diffToAbove = target.ValueAt(timing) - above.ValueAt(timing);
            float diffToBelow = target.ValueAt(timing) - below.ValueAt(timing);

            if (lastDiffToAbove < 0 && diffToAbove >= 0)
            {
                Dispatch(timing);
            }

            if (lastDiffToBelow > 0 && diffToBelow <= 0)
            {
                Dispatch(timing);
            }

            lastDiffToAbove = diffToAbove;
            lastDiffToBelow = diffToBelow;
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(target),
                serialization.AddUnitAndGetId(above),
                serialization.AddUnitAndGetId(below),
                serialization.AddUnitAndGetId(TriggerDispatch.Value),
                serialization.AddUnitAndGetId(TriggerDispatch.Duration),
                TriggerDispatch.EasingString,
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            target = deserialization.GetUnitFromId<ValueChannel>(properties[0]);
            above = deserialization.GetUnitFromId<ValueChannel>(properties[1]);
            below = deserialization.GetUnitFromId<ValueChannel>(properties[2]);
            TriggerDispatch = new TriggerValueDispatch
            {
                Value = deserialization.GetUnitFromId<ValueChannel>(properties[3]),
                Duration = deserialization.GetUnitFromId<ValueChannel>(properties[4]),
                EasingString = (string)properties[5],
                Easing = Easing.FromString((string)properties[5]),
            };
        }
    }
}