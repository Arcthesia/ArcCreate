using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolDeserialization
    {
        private readonly Scene scene;
        private readonly PostProcessing postProcessing;
        private readonly List<SerializedUnit> serializedUnits;
        private readonly ISerializableUnit[] deserialized;

        public ScenecontrolDeserialization(Scene scene, PostProcessing postProcessing, List<SerializedUnit> serializedUnits)
        {
            this.serializedUnits = serializedUnits;
            this.scene = scene;
            this.postProcessing = postProcessing;
            deserialized = new ISerializableUnit[serializedUnits.Count];
            for (int i = 0; i < serializedUnits.Count; i++)
            {
                deserialized[i] = GetUnitFromId(i);
            }
        }

        public List<ISerializableUnit> Result => deserialized.ToList();

        public ISerializableUnit GetUnitFromId(int id)
        {
            if (deserialized[id] != null)
            {
                return deserialized[id];
            }

            SerializedUnit serializedChannel = serializedUnits[id];
            ISerializableUnit result = GetUnitFromType(serializedChannel.Type);
            result.DeserializeProperties(serializedChannel.Properties, this);
            return result;
        }

        public T GetUnitFromId<T>(object obj)
            where T : class, ISerializableUnit
        {
            if (obj == null)
            {
                return null;
            }

            int id = Convert.ToInt32(obj);
            return GetUnitFromId(id) as T;
        }

        public ISerializableUnit GetUnitFromType(string type)
        {
            switch (type)
            {
                case "context":
                    return Services.Scenecontrol.Context;

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
                case "channel.text.constant":
                    return new ConstantTextChannel();
                case "channel.text.key":
                    return new KeyTextChannel();
                case "channel.text.concat":
                    return new ConcatTextChannel();
                case "channel.text.value":
                    return new ValueToTextChannel();

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
                    ISceneController c = scene.CreateFromTypeName(type);
                    if (c != null)
                    {
                        return c;
                    }

                    ISceneController p = postProcessing.CreateFromTypeName(type);
                    if (p != null)
                    {
                        return p;
                    }

                    throw new Exception($"Could not resolve object type {type}");
            }
        }
    }
}