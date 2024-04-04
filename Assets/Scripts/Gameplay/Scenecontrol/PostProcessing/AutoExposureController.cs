using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class AutoExposureController : PostProcessingController<AutoExposure>
    {
        public ValueChannel FilteringFrom { get; set; }

        public ValueChannel FilteringTo { get; set; }

        public ValueChannel MinLuminance { get; set; }

        public ValueChannel MaxLuminance { get; set; }

        public ValueChannel KeyValue { get; set; }

        public ValueChannel SpeedUp { get; set; }

        public ValueChannel SpeedDown { get; set; }

        public float DefaultFilteringFrom { get; set; }

        public float DefaultFilteringTo { get; set; }

        public float DefaultMinLuminance { get; set; }

        public float DefaultMaxLuminance { get; set; }

        public float DefaultKeyValue { get; set; }

        public EyeAdaptation DefaultEyeAdaptation { get; set; }

        public float DefaultSpeedUp { get; set; }

        public float DefaultSpeedDown { get; set; }

        public override void EnableEffect(string[] effects)
        {
            TargetEffect.enabled.Override(true);
            foreach (string effect in effects)
            {
                switch (effect.ToLower())
                {
                    case "filtering": TargetEffect.filtering.overrideState = true; break;
                    case "minluminance": TargetEffect.minLuminance.overrideState = true; break;
                    case "maxluminance": TargetEffect.maxLuminance.overrideState = true; break;
                    case "keyvalue": TargetEffect.keyValue.overrideState = true; break;
                    case "speedup": TargetEffect.speedUp.overrideState = true; break;
                    case "speeddown": TargetEffect.speedDown.overrideState = true; break;
                }
            }
        }

        public AutoExposureController SetEyeAdaptation(int mode)
        {
            TargetEffect.eyeAdaptation.Override((EyeAdaptation)mode);
            return this;
        }

        public override void UpdateController(int timing)
        {
            TargetEffect.filtering.value = new Vector2(FilteringFrom.ValueAt(timing), FilteringTo.ValueAt(timing));
            TargetEffect.minLuminance.value = MinLuminance.ValueAt(timing);
            TargetEffect.maxLuminance.value = MaxLuminance.ValueAt(timing);
            TargetEffect.keyValue.value = KeyValue.ValueAt(timing);
            TargetEffect.speedUp.value = SpeedUp.ValueAt(timing);
            TargetEffect.speedDown.value = SpeedDown.ValueAt(timing);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                TargetEffect.enabled.value,
                TargetEffect.filtering.overrideState,
                TargetEffect.minLuminance.overrideState,
                TargetEffect.maxLuminance.overrideState,
                TargetEffect.keyValue.overrideState,
                TargetEffect.speedUp.overrideState,
                TargetEffect.speedDown.overrideState,
                serialization.AddUnitAndGetId(FilteringFrom),
                serialization.AddUnitAndGetId(FilteringTo),
                serialization.AddUnitAndGetId(MinLuminance),
                serialization.AddUnitAndGetId(MaxLuminance),
                serialization.AddUnitAndGetId(KeyValue),
                serialization.AddUnitAndGetId(SpeedUp),
                serialization.AddUnitAndGetId(SpeedDown),
                (int)TargetEffect.eyeAdaptation.value,
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int offset = 0;
            TargetEffect.enabled.Override((bool)properties[offset++] && !Settings.DisableAdvancedGraphics.Value);
            TargetEffect.filtering.overrideState = (bool)properties[offset++];
            TargetEffect.minLuminance.overrideState = (bool)properties[offset++];
            TargetEffect.maxLuminance.overrideState = (bool)properties[offset++];
            TargetEffect.keyValue.overrideState = (bool)properties[offset++];
            TargetEffect.speedUp.overrideState = (bool)properties[offset++];
            TargetEffect.speedDown.overrideState = (bool)properties[offset++];
            FilteringFrom = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            FilteringTo = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MinLuminance = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MaxLuminance = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            KeyValue = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            SpeedUp = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            SpeedDown = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            TargetEffect.eyeAdaptation.Override((EyeAdaptation)(int)(long)properties[offset++]);
        }

        protected override void SetupDefault()
        {
            DefaultFilteringFrom = TargetEffect.filtering.value.x;
            DefaultFilteringTo = TargetEffect.filtering.value.y;
            DefaultMinLuminance = TargetEffect.minLuminance.value;
            DefaultMaxLuminance = TargetEffect.maxLuminance.value;
            DefaultKeyValue = TargetEffect.keyValue.value;
            DefaultEyeAdaptation = TargetEffect.eyeAdaptation.value;
            DefaultSpeedUp = TargetEffect.speedUp.value;
            DefaultSpeedDown = TargetEffect.speedDown.value;
        }

        protected override void Reset()
        {
            FilteringFrom = new ConstantChannel(DefaultFilteringFrom);
            FilteringTo = new ConstantChannel(DefaultFilteringTo);
            MinLuminance = new ConstantChannel(DefaultMinLuminance);
            MaxLuminance = new ConstantChannel(DefaultMaxLuminance);
            KeyValue = new ConstantChannel(DefaultKeyValue);
            SpeedUp = new ConstantChannel(DefaultSpeedUp);
            SpeedDown = new ConstantChannel(DefaultSpeedDown);
            TargetEffect.filtering.overrideState = false;
            TargetEffect.minLuminance.overrideState = false;
            TargetEffect.maxLuminance.overrideState = false;
            TargetEffect.keyValue.overrideState = false;
            TargetEffect.eyeAdaptation.overrideState = false;
            TargetEffect.speedUp.overrideState = false;
            TargetEffect.speedDown.overrideState = false;
            TargetEffect.eyeAdaptation.value = DefaultEyeAdaptation;
        }
    }
}