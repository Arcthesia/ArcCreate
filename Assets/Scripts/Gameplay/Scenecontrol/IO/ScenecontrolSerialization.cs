using System;
using System.Collections.Generic;

namespace ArcCreate.Gameplay.Scenecontrol
{
    public class ScenecontrolSerialization
    {
        private readonly List<ISerializableUnit> units = new List<ISerializableUnit>();
        private readonly List<SerializedUnit> serializedUnits;
        private readonly Dictionary<ISerializableUnit, int> idLookup = new Dictionary<ISerializableUnit, int>();

        public List<SerializedUnit> Result => serializedUnits;

        public ScenecontrolSerialization()
        {
            var versioning = new ScenecontrolVersioning(EnabledFeatures.All);
            units.Add(versioning);
            idLookup.Add(versioning, 0);
            serializedUnits = new List<SerializedUnit> {
                new SerializedUnit
                {
                    Type = GetTypeFromUnit(versioning),
                    Properties = versioning.SerializeProperties(this),
                }};
        }

        public int? AddUnitAndGetId(ISerializableUnit unit)
        {
            if (unit == null)
            {
                return null;
            }

            if (idLookup.TryGetValue(unit, out int id))
            {
                return id;
            }

            units.Add(unit);
            id = units.Count - 1;
            idLookup.Add(unit, id);
            SerializedUnit serialized = default;
            serializedUnits.Add(serialized);
            serializedUnits[id] = new SerializedUnit
            {
                Type = GetTypeFromUnit(unit),
                Properties = unit.SerializeProperties(this),
            };

            return id;
        }

        public string GetTypeFromUnit(ISerializableUnit unit)
        {
            switch (unit)
            {
                case ScenecontrolVersioning versioning:
                    return "versioning";
                case Context context:
                    return "context";

                // Channels
                case KeyChannel key:
                    return "channel.key";
                case FFTChannel fft:
                    return "channel.fft";
                case ClampChannel clamp:
                    return "channel.clamp";
                case ConditionalChannel condition:
                    return "channel.condition";
                case ConstantChannel constant:
                    return "channel.const";
                case CosChannel cos:
                    return "channel.cos";
                case ExpChannel exp:
                    return "channel.exp";
                case InverseChannel inverse:
                    return "channel.inverse";
                case MaxChannel max:
                    return "channel.max";
                case MinChannel min:
                    return "channel.min";
                case NegateChannel negate:
                    return "channel.negate";
                case NoiseChannel noise:
                    return "channel.noise";
                case ProductChannel product:
                    return "channel.product";
                case ModuloChannel moduluo:
                    return "channel.modulo";
                case RandomChannel random:
                    return "channel.random";
                case SawChannel saw:
                    return "channel.saw";
                case SineChannel sine:
                    return "channel.sine";
                case SumChannel sum:
                    return "channel.sum";

                // Trigger channels
                case AccumulatingTriggerChannel accum:
                    return "channel.trigger.accumulate";
                case LoopingTriggerChannel loop:
                    return "channel.trigger.loop";
                case StackingTriggerChannel stack:
                    return "channel.trigger.stack";
                case SettingTriggerChannel setting:
                    return "channel.trigger.set";

                // String channels
                case KeyStringChannel keystring:
                    return "channel.string.key";
                case ConstantTextChannel constanttext:
                    return "channel.text.constant";
                case KeyTextChannel keytext:
                    return "channel.text.key";
                case ConcatTextChannel concat:
                    return "channel.text.concat";
                case ValueToTextChannel valuetext:
                    return "channel.text.value";

                // Contexts
                case DropRateChannel droprate:
                    return "channel.context.droprate";
                case GlobalOffsetChannel goffset:
                    return "channel.context.globaloffset";
                case CurrentScoreChannel score:
                    return "channel.context.currentscore";
                case CurrentComboChannel combo:
                    return "channel.context.currentcombo";
                case CurrentTimingChannel timing:
                    return "channel.context.currenttiming";
                case ScreenWidthChannel swidth:
                    return "channel.context.screenwidth";
                case ScreenHeightChannel sheight:
                    return "channel.context.screenheight";
                case ScreenIs16By9Channel is16by9:
                    return "channel.context.is16by9";
                case BPMChannel bpm:
                    return "channel.context.bpm";
                case DivisorChannel divisor:
                    return "channel.context.divisor";
                case FloorPositionChannel fp:
                    return "channel.context.floorposition";

                // Triggers
                case JudgementTrigger tjudgement:
                    return "trigger.judgement";
                case ObserveTrigger tobserve:
                    return "trigger.observe";
                default:
                    if (unit is ISceneController controller)
                    {
                        string name = controller?.SerializedType;
                        if (!string.IsNullOrEmpty(name))
                        {
                            return name;
                        }
                    }

                    throw new Exception($"Could not get type of object: {unit.GetType().Name}");
            }
        }
    }
}