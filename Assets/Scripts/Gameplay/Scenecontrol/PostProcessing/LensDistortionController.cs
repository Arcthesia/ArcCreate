using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class LensDistortionController : PostProcessingController<LensDistortion>
    {
        public ValueChannel Intensity { get; set; }

        public ValueChannel IntensityX { get; set; }

        public ValueChannel IntensityY { get; set; }

        public ValueChannel CenterX { get; set; }

        public ValueChannel CenterY { get; set; }

        public ValueChannel Scale { get; set; }

        public float DefaultIntensity { get; set; }

        public float DefaultIntensityX { get; set; }

        public float DefaultIntensityY { get; set; }

        public float DefaultCenterX { get; set; }

        public float DefaultCenterY { get; set; }

        public float DefaultScale { get; set; }

        public override void EnableEffect(string[] effects)
        {
            TargetEffect.enabled.Override(true);
            foreach (string effect in effects)
            {
                switch (effect.ToLower())
                {
                    case "intensity": TargetEffect.intensity.overrideState = true; break;
                    case "intensityx": TargetEffect.intensityX.overrideState = true; break;
                    case "intensityy": TargetEffect.intensityY.overrideState = true; break;
                    case "centerx": TargetEffect.centerX.overrideState = true; break;
                    case "centery": TargetEffect.centerY.overrideState = true; break;
                    case "scale": TargetEffect.scale.overrideState = true; break;
                }
            }
        }

        public override void UpdateController(int timing)
        {
            TargetEffect.intensity.value = Intensity.ValueAt(timing);
            TargetEffect.intensityX.value = IntensityX.ValueAt(timing);
            TargetEffect.intensityY.value = IntensityY.ValueAt(timing);
            TargetEffect.centerX.value = CenterX.ValueAt(timing);
            TargetEffect.centerY.value = CenterY.ValueAt(timing);
            TargetEffect.scale.value = Scale.ValueAt(timing);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                TargetEffect.enabled.value,
                TargetEffect.intensity.overrideState,
                TargetEffect.intensityX.overrideState,
                TargetEffect.intensityY.overrideState,
                TargetEffect.centerX.overrideState,
                TargetEffect.centerY.overrideState,
                TargetEffect.scale.overrideState,
                serialization.AddUnitAndGetId(Intensity),
                serialization.AddUnitAndGetId(IntensityX),
                serialization.AddUnitAndGetId(IntensityY),
                serialization.AddUnitAndGetId(CenterX),
                serialization.AddUnitAndGetId(CenterY),
                serialization.AddUnitAndGetId(Scale),
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int offset = 0;
            TargetEffect.enabled.Override((bool)properties[offset++] && !Settings.DisableAdvancedGraphics.Value);
            TargetEffect.intensity.overrideState = (bool)properties[offset++];
            TargetEffect.intensityX.overrideState = (bool)properties[offset++];
            TargetEffect.intensityY.overrideState = (bool)properties[offset++];
            TargetEffect.centerX.overrideState = (bool)properties[offset++];
            TargetEffect.centerY.overrideState = (bool)properties[offset++];
            TargetEffect.scale.overrideState = (bool)properties[offset++];
            Intensity = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            IntensityX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            IntensityY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            CenterX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            CenterY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Scale = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
        }

        protected override void SetupDefault()
        {
            DefaultIntensity = TargetEffect.intensity.value;
            DefaultIntensityX = TargetEffect.intensityX.value;
            DefaultIntensityY = TargetEffect.intensityY.value;
            DefaultCenterX = TargetEffect.centerX.value;
            DefaultCenterY = TargetEffect.centerY.value;
            DefaultScale = TargetEffect.scale.value;
        }

        protected override void Reset()
        {
            Intensity = new ConstantChannel(DefaultIntensity);
            IntensityX = new ConstantChannel(DefaultIntensityX);
            IntensityY = new ConstantChannel(DefaultIntensityY);
            CenterX = new ConstantChannel(DefaultCenterX);
            CenterY = new ConstantChannel(DefaultCenterY);
            Scale = new ConstantChannel(DefaultScale);
            TargetEffect.intensity.overrideState = false;
            TargetEffect.intensityX.overrideState = false;
            TargetEffect.intensityY.overrideState = false;
            TargetEffect.centerX.overrideState = false;
            TargetEffect.centerY.overrideState = false;
            TargetEffect.scale.overrideState = false;
        }
    }
}