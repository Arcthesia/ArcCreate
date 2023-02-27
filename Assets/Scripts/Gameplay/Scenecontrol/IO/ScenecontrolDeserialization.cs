using System;
using System.Collections.Generic;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolDeserialization
    {
        private readonly List<SerializedUnit> serializedChannels;

        public ScenecontrolDeserialization(List<SerializedUnit> serializedChannels)
        {
            this.serializedChannels = serializedChannels;
        }

        public SerializableUnit GetUnitFromId(int id)
        {
            SerializedUnit serializedChannel = serializedChannels[id];

            SerializableUnit result = GetUnitFromType(serializedChannel.Type);
            result.DeserializeProperties(serializedChannel.Properties, this);
            return result;
        }

        public SerializableUnit GetUnitFromType(string type)
        {
            switch (type)
            {
                // Channels
                case "channel.key":
                    return new KeyChannel();
                case "channel.fft":
                    return new FFTChannel();
                case "channel.clamp":
                    return new ClampChannel();
                case "channel.condition":
                    return new ConditionalChannel();
                case "channel.const":
                    return new ConstantChannel();
                case "channel.cos":
                    return new CosChannel();
                case "channel.exp":
                    return new ExpChannel();
                case "channel.max":
                    return new MaxChannel();
                case "channel.min":
                    return new MinChannel();
                case "channel.noise":
                    return new NoiseChannel();
                case "channel.random":
                    return new RandomChannel();
                case "channel.saw":
                    return new SawChannel();
                case "channel.sine":
                    return new SineChannel();
                case "channel.trigger.accumulate":
                    return new AccumulatingTriggerChannel();
                case "channel.trigger.loop":
                    return new LoopingTriggerChannel();
                case "channel.trigger.stack":
                    return new StackingTriggerChannel();
                case "channel.trigger.set":
                    return new SettingTriggerChannel();

                // Triggers
                case "trigger.judgement":
                    return new JudgementTrigger();
                case "trigger.observe":
                    return new ObserveTrigger();
                default:
                    throw new Exception($"Could not resolve object type: {type}");
            }
        }
    }
}