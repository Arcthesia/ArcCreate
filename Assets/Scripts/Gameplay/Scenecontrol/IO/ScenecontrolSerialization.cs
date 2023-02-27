using System;
using System.Collections.Generic;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolSerialization
    {
        private readonly List<SerializableUnit> channels = new List<SerializableUnit>();
        private readonly List<SerializedUnit> serializedChannels = new List<SerializedUnit>();
        private readonly Dictionary<SerializableUnit, int> channelIdLookup = new Dictionary<SerializableUnit, int>();

        public int AddUnitAndGetId(SerializableUnit channel)
        {
            if (channelIdLookup.TryGetValue(channel, out int id))
            {
                return id;
            }

            channels.Add(channel);
            id = channels.Count - 1;
            channelIdLookup.Add(channel, id);

            serializedChannels.Add(new SerializedUnit
            {
                Type = GetTypeFromUnit(channel),
                Properties = channel.SerializeProperties(this),
            });

            return id;
        }

        public string GetTypeFromUnit(SerializableUnit unit)
        {
            switch (unit)
            {
                // Channels
                case KeyChannel:
                    return "channel.key";
                case FFTChannel:
                    return "channel.fft";
                case ClampChannel:
                    return "channel.clamp";
                case ConditionalChannel:
                    return "channel.condition";
                case ConstantChannel:
                    return "channel.const";
                case CosChannel:
                    return "channel.cos";
                case ExpChannel:
                    return "channel.exp";
                case MaxChannel:
                    return "channel.max";
                case MinChannel:
                    return "channel.min";
                case NoiseChannel:
                    return "channel.noise";
                case RandomChannel:
                    return "channel.random";
                case SawChannel:
                    return "channel.saw";
                case SineChannel:
                    return "channel.sine";
                case AccumulatingTriggerChannel:
                    return "channel.trigger.accumulate";
                case LoopingTriggerChannel:
                    return "channel.trigger.loop";
                case StackingTriggerChannel:
                    return "channel.trigger.stack";
                case SettingTriggerChannel:
                    return "channel.trigger.set";

                // Triggers
                case JudgementTrigger:
                    return "trigger.judgement";
                case ObserveTrigger:
                    return "trigger.observe";
                default:
                    throw new Exception($"Could not get type of object: {unit.GetType().Name}");
            }
        }
    }
}