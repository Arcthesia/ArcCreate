using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class JudgementTrigger : Trigger
    {
        private bool triggerOnMax = false;
        private bool triggerOnPerfectEarly = false;
        private bool triggerOnPerfectLate = false;
        private bool triggerOnGoodEarly = false;
        private bool triggerOnGoodLate = false;
        private bool triggerOnMissEarly = false;
        private bool triggerOnMissLate = false;

        [EmmyDoc("Sets the channel to listen to Max judge event")]
        public JudgementTrigger OnMax()
        {
            triggerOnMax = true;
            return this;
        }

        [EmmyDoc("Sets the channel to listen to Perfect judge event")]
        public JudgementTrigger OnPerfect()
        {
            triggerOnPerfectEarly = true;
            triggerOnPerfectLate = true;
            triggerOnMax = true;
            return this;
        }

        [EmmyDoc("Sets the channel to listen to Early Perfect judge event")]
        public JudgementTrigger OnPerfectEarly()
        {
            triggerOnPerfectEarly = true;
            return this;
        }

        [EmmyDoc("Sets the channel to listen to Late Perfect judge event")]
        public JudgementTrigger OnPerfectLate()
        {
            triggerOnPerfectLate = true;
            return this;
        }

        [EmmyDoc("Sets the channel to listen to Good judge event")]
        public JudgementTrigger OnGood()
        {
            triggerOnGoodEarly = true;
            triggerOnGoodLate = true;
            return this;
        }

        [EmmyDoc("Sets the channel to listen to Early Good judge event")]
        public JudgementTrigger OnGoodEarly()
        {
            triggerOnGoodEarly = true;
            return this;
        }

        [EmmyDoc("Sets the channel to listen to Late Good judge event")]
        public JudgementTrigger OnGoodLate()
        {
            triggerOnGoodLate = true;
            return this;
        }

        [EmmyDoc("Sets the channel to listen to Miss judge event")]
        public JudgementTrigger OnMiss()
        {
            triggerOnMissEarly = true;
            triggerOnMissLate = true;
            return this;
        }

        [EmmyDoc("Sets the channel to listen to Early Miss judge event")]
        public JudgementTrigger OnMissEarly()
        {
            triggerOnMissEarly = true;
            return this;
        }

        [EmmyDoc("Sets the channel to listen to Late Miss judge event")]
        public JudgementTrigger OnMissLate()
        {
            triggerOnMissLate = true;
            return this;
        }

        [EmmyDoc("Sets the value to send to the TriggerChannel bound to this trigger.")]
        public JudgementTrigger Dispatch(
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
            };

            return this;
        }

        public override void Poll(int timing)
        {
            List<JudgementResult> judgements = Services.Score.GetJudgementsThisFrame();
            for (int i = 0; i < judgements.Count; i++)
            {
                JudgementResult res = judgements[i];
                switch (res)
                {
                    case JudgementResult.MissEarly:
                        if (triggerOnMissEarly)
                        {
                            Dispatch(timing);
                        }

                        break;
                    case JudgementResult.GoodEarly:
                        if (triggerOnGoodEarly)
                        {
                            Dispatch(timing);
                        }

                        break;
                    case JudgementResult.PerfectEarly:
                        if (triggerOnPerfectEarly)
                        {
                            Dispatch(timing);
                        }

                        break;
                    case JudgementResult.Max:
                        if (triggerOnMax)
                        {
                            Dispatch(timing);
                        }

                        break;
                    case JudgementResult.PerfectLate:
                        if (triggerOnPerfectLate)
                        {
                            Dispatch(timing);
                        }

                        break;
                    case JudgementResult.GoodLate:
                        if (triggerOnGoodLate)
                        {
                            Dispatch(timing);
                        }

                        break;
                    case JudgementResult.MissLate:
                        if (triggerOnMissLate)
                        {
                            Dispatch(timing);
                        }

                        break;
                }
            }
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                triggerOnMax,
                triggerOnPerfectEarly,
                triggerOnPerfectLate,
                triggerOnGoodEarly,
                triggerOnGoodLate,
                triggerOnMissEarly,
                triggerOnMissLate,
                serialization.AddUnitAndGetId(TriggerDispatch.Value),
                serialization.AddUnitAndGetId(TriggerDispatch.Duration),
                TriggerDispatch.EasingString,
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            triggerOnMax = (bool)properties[0];
            triggerOnPerfectEarly = (bool)properties[1];
            triggerOnPerfectLate = (bool)properties[2];
            triggerOnGoodEarly = (bool)properties[3];
            triggerOnGoodLate = (bool)properties[4];
            triggerOnMissEarly = (bool)properties[5];
            triggerOnMissLate = (bool)properties[6];
            TriggerDispatch = new TriggerValueDispatch
            {
                Value = deserialization.GetUnitFromId<ValueChannel>(properties[7]),
                Duration = deserialization.GetUnitFromId<ValueChannel>(properties[8]),
                EasingString = (string)properties[9],
                Easing = Easing.FromString((string)properties[9]),
            };
        }
    }
}