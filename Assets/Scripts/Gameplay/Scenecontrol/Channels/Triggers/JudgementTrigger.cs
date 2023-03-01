using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class JudgementTrigger : Trigger
    {
        private bool triggerOnMax = false;
        private bool triggerOnPureEarly = false;
        private bool triggerOnPureLate = false;
        private bool triggerOnFarEarly = false;
        private bool triggerOnFarLate = false;
        private bool triggerOnLostEarly = false;
        private bool triggerOnLostLate = false;

        public JudgementTrigger OnMax()
        {
            triggerOnMax = true;
            return this;
        }

        public JudgementTrigger OnPure()
        {
            triggerOnPureEarly = true;
            triggerOnPureLate = true;
            triggerOnMax = true;
            return this;
        }

        public JudgementTrigger OnPureEarly()
        {
            triggerOnPureEarly = true;
            return this;
        }

        public JudgementTrigger OnPureLate()
        {
            triggerOnPureLate = true;
            return this;
        }

        public JudgementTrigger OnFar()
        {
            triggerOnFarEarly = true;
            triggerOnFarLate = true;
            return this;
        }

        public JudgementTrigger OnFarEarly()
        {
            triggerOnFarEarly = true;
            return this;
        }

        public JudgementTrigger OnFarLate()
        {
            triggerOnFarLate = true;
            return this;
        }

        public JudgementTrigger OnLost()
        {
            triggerOnLostEarly = true;
            triggerOnLostLate = true;
            return this;
        }

        public JudgementTrigger OnLostEarly()
        {
            triggerOnLostEarly = true;
            return this;
        }

        public JudgementTrigger OnLostLate()
        {
            triggerOnLostLate = true;
            return this;
        }

        public JudgementTrigger Dispatch(ValueChannel value, ValueChannel duration = null, string easing = null)
        {
            TriggerValueDispatch dispatch = new TriggerValueDispatch
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
                    case JudgementResult.LostEarly:
                        if (triggerOnLostEarly)
                        {
                            Dispatch(timing);
                        }

                        break;
                    case JudgementResult.FarEarly:
                        if (triggerOnFarEarly)
                        {
                            Dispatch(timing);
                        }

                        break;
                    case JudgementResult.PureEarly:
                        if (triggerOnPureEarly)
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
                    case JudgementResult.PureLate:
                        if (triggerOnPureLate)
                        {
                            Dispatch(timing);
                        }

                        break;
                    case JudgementResult.FarLate:
                        if (triggerOnFarLate)
                        {
                            Dispatch(timing);
                        }

                        break;
                    case JudgementResult.LostLate:
                        if (triggerOnLostLate)
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
                triggerOnPureEarly,
                triggerOnPureLate,
                triggerOnFarEarly,
                triggerOnFarLate,
                triggerOnLostEarly,
                triggerOnLostLate,
                serialization.AddUnitAndGetId(TriggerDispatch.Value),
                serialization.AddUnitAndGetId(TriggerDispatch.Duration),
                TriggerDispatch.EasingString,
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            triggerOnMax = (bool)properties[0];
            triggerOnPureEarly = (bool)properties[1];
            triggerOnPureLate = (bool)properties[2];
            triggerOnFarEarly = (bool)properties[3];
            triggerOnFarLate = (bool)properties[4];
            triggerOnLostEarly = (bool)properties[5];
            triggerOnLostLate = (bool)properties[6];
            TriggerDispatch = new TriggerValueDispatch
            {
                Value = deserialization.GetUnitFromId((int)properties[7]) as ValueChannel,
                Duration = deserialization.GetUnitFromId((int)properties[8]) as ValueChannel,
                EasingString = (string)properties[9],
                Easing = Easing.FromString((string)properties[10]),
            };
        }
    }
}