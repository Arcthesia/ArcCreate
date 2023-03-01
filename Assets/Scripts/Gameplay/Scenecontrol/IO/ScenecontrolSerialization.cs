using System;
using System.Collections.Generic;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolSerialization
    {
        private readonly List<ISerializableUnit> channels = new List<ISerializableUnit>();
        private readonly List<SerializedUnit> serializedChannels = new List<SerializedUnit>();
        private readonly Dictionary<ISerializableUnit, int> channelIdLookup = new Dictionary<ISerializableUnit, int>();

        public int AddUnitAndGetId(ISerializableUnit channel)
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

        public string GetTypeFromUnit(ISerializableUnit unit)
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
                case InverseChannel:
                    return "channel.inverse";
                case MaxChannel:
                    return "channel.max";
                case MinChannel:
                    return "channel.min";
                case NegateChannel:
                    return "channel.negate";
                case NoiseChannel:
                    return "channel.noise";
                case ProductChannel:
                    return "channel.product";
                case RandomChannel:
                    return "channel.random";
                case SawChannel:
                    return "channel.saw";
                case SineChannel:
                    return "channel.sine";
                case SumChannel:
                    return "channel.sum";

                // Trigger channels
                case AccumulatingTriggerChannel:
                    return "channel.trigger.accumulate";
                case LoopingTriggerChannel:
                    return "channel.trigger.loop";
                case StackingTriggerChannel:
                    return "channel.trigger.stack";
                case SettingTriggerChannel:
                    return "channel.trigger.set";

                // String channels
                case KeyStringChannel:
                    return "channel.string.key";
                case KeyTextChannel:
                    return "channel.text.key";
                case ConcatTextChannel:
                    return "channel.text.concat";

                // Contexts
                case DropRateChannel:
                    return "channel.context.droprate";
                case GlobalOffsetChannel:
                    return "channel.context.globaloffset";
                case CurrentScoreChannel:
                    return "channel.context.currentscore";
                case CurrentComboChannel:
                    return "channel.context.currentcombo";
                case CurrentTimingChannel:
                    return "channel.context.currenttiming";
                case ScreenWidthChannel:
                    return "channel.context.screenwidth";
                case ScreenHeightChannel:
                    return "channel.context.screenheight";
                case ScreenIs16By9Channel:
                    return "channel.context.is16by9";
                case BPMChannel:
                    return "channel.context.bpm";
                case DivisorChannel:
                    return "channel.context.divisor";
                case FloorPositionChannel:
                    return "channel.context.floorposition";

                // Triggers
                case JudgementTrigger:
                    return "trigger.judgement";
                case ObserveTrigger:
                    return "trigger.observe";
                default:
                    Controller controller = unit as Controller;
                    string name = controller == null ? null : controller.SerializedType;
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new Exception($"Could not get type of object: {unit.GetType().Name}");
                    }

                    return name;
            }
        }
    }
}