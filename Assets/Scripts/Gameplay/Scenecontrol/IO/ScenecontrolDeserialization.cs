using System;
using System.Collections.Generic;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolDeserialization
    {
        private readonly Scene scene;
        private readonly List<SerializedUnit> serializedChannels;

        public ScenecontrolDeserialization(Scene scene, List<SerializedUnit> serializedChannels)
        {
            this.serializedChannels = serializedChannels;
            this.scene = scene;
        }

        public ISerializableUnit GetUnitFromId(int id)
        {
            SerializedUnit serializedChannel = serializedChannels[id];

            ISerializableUnit result = GetUnitFromType(serializedChannel.Type);
            result.DeserializeProperties(serializedChannel.Properties, this);
            return result;
        }

        public ISerializableUnit GetUnitFromType(string type)
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
                case "channel.inverse":
                    return new InverseChannel();
                case "channel.max":
                    return new MaxChannel();
                case "channel.min":
                    return new MinChannel();
                case "channel.negate":
                    return new NegateChannel();
                case "channel.noise":
                    return new NoiseChannel();
                case "channel.product":
                    return new ProductChannel();
                case "channel.random":
                    return new RandomChannel();
                case "channel.saw":
                    return new SawChannel();
                case "channel.sine":
                    return new SineChannel();
                case "channel.sum":
                    return new SumChannel();

                // Triggers channels
                case "channel.trigger.accumulate":
                    return new AccumulatingTriggerChannel();
                case "channel.trigger.loop":
                    return new LoopingTriggerChannel();
                case "channel.trigger.stack":
                    return new StackingTriggerChannel();
                case "channel.trigger.set":
                    return new SettingTriggerChannel();

                // String channels
                case "channel.string.key":
                    return new KeyStringChannel();
                case "channel.text.key":
                    return new KeyTextChannel();
                case "channel.text.concat":
                    return new ConcatTextChannel();

                // Contexts
                case "channel.context.droprate":
                    return new DropRateChannel();
                case "channel.context.globaloffset":
                    return new GlobalOffsetChannel();
                case "channel.context.currentscore":
                    return new CurrentScoreChannel();
                case "channel.context.currentcombo":
                    return new CurrentComboChannel();
                case "channel.context.currenttiming":
                    return new CurrentTimingChannel();
                case "channel.context.screenwidth":
                    return new ScreenWidthChannel();
                case "channel.context.screenheight":
                    return new ScreenHeightChannel();
                case "channel.context.is16by9":
                    return new ScreenIs16By9Channel();
                case "channel.context.bpm":
                    return new BPMChannel();
                case "channel.context.divisor":
                    return new DivisorChannel();
                case "channel.context.floorposition":
                    return new FloorPositionChannel();

                // Triggers
                case "trigger.judgement":
                    return new JudgementTrigger();
                case "trigger.observe":
                    return new ObserveTrigger();
                default:
                    Controller c = scene.CreateFromTypeName(type);
                    if (c == null)
                    {
                        throw new Exception($"Could not resolve object type: {type}");
                    }

                    return c;
            }
        }
    }
}