using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class MotionBlurController : PostProcessingController<MotionBlur>
    {
        public ValueChannel ShutterAngle { get; set; }

        public ValueChannel SampleCount { get; set; }

        public float DefaultShutterAngle { get; set; }

        public float DefaultSampleCount { get; set; }

        public override void EnableEffect(string[] effects)
        {
            TargetEffect.enabled.Override(true);
            foreach (string effect in effects)
            {
                switch (effect.ToLower())
                {
                    case "shutterangle": TargetEffect.shutterAngle.overrideState = true; break;
                    case "samplecount": TargetEffect.sampleCount.overrideState = true; break;
                }
            }
        }

        public override void UpdateController(int timing)
        {
            TargetEffect.shutterAngle.value = ShutterAngle.ValueAt(timing);
            TargetEffect.sampleCount.value = (int)SampleCount.ValueAt(timing);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                TargetEffect.enabled.value,
                TargetEffect.shutterAngle.overrideState,
                TargetEffect.sampleCount.overrideState,
                serialization.AddUnitAndGetId(ShutterAngle),
                serialization.AddUnitAndGetId(SampleCount),
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int offset = 0;
            TargetEffect.enabled.Override((bool)properties[offset++] && !Settings.DisableAdvancedGraphics.Value);
            TargetEffect.shutterAngle.overrideState = (bool)properties[offset++];
            TargetEffect.sampleCount.overrideState = (bool)properties[offset++];
            ShutterAngle = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            SampleCount = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
        }

        protected override void SetupDefault()
        {
            DefaultShutterAngle = TargetEffect.shutterAngle.value;
            DefaultSampleCount = TargetEffect.sampleCount.value;
        }

        protected override void Reset()
        {
            ShutterAngle = new ConstantChannel(DefaultShutterAngle);
            SampleCount = new ConstantChannel(DefaultSampleCount);
            TargetEffect.shutterAngle.overrideState = false;
            TargetEffect.sampleCount.overrideState = false;
        }
    }
}