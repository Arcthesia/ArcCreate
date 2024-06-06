using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class GrainController : PostProcessingController<Grain>
    {
        public ValueChannel Intensity { get; set; }

        public ValueChannel Size { get; set; }

        public ValueChannel LumContrib { get; set; }

        public bool DefaultColored { get; set; }

        public float DefaultIntensity { get; set; }

        public float DefaultSize { get; set; }

        public float DefaultLumContrib { get; set; }

        public override void EnableEffect(string[] effects)
        {
            TargetEffect.enabled.Override(true);
            foreach (string effect in effects)
            {
                switch (effect.ToLower())
                {
                    case "intensity": TargetEffect.intensity.overrideState = true; break;
                    case "size": TargetEffect.size.overrideState = true; break;
                    case "lumcontrib": TargetEffect.lumContrib.overrideState = true; break;
                }
            }
        }

        public void SetColored(bool colored)
        {
            TargetEffect.colored.Override(colored);
        }

        public override void UpdateController(int timing)
        {
            TargetEffect.intensity.value = Intensity.ValueAt(timing);
            TargetEffect.size.value = Size.ValueAt(timing);
            TargetEffect.lumContrib.value = LumContrib.ValueAt(timing);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                TargetEffect.enabled.value,
                TargetEffect.intensity.overrideState,
                TargetEffect.size.overrideState,
                TargetEffect.lumContrib.overrideState,
                serialization.AddUnitAndGetId(Intensity),
                serialization.AddUnitAndGetId(Size),
                serialization.AddUnitAndGetId(LumContrib),
            };
        }

        public override void DeserializeProperties(List<object> properties, EnabledFeatures features, ScenecontrolDeserialization deserialization)
        {
            int offset = 0;
            TargetEffect.enabled.Override((bool)properties[offset++] && !Settings.DisableAdvancedGraphics.Value);
            TargetEffect.intensity.overrideState = (bool)properties[offset++];
            TargetEffect.size.overrideState = (bool)properties[offset++];
            TargetEffect.lumContrib.overrideState = (bool)properties[offset++];
            Intensity = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Size = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            LumContrib = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
        }

        protected override void SetupDefault()
        {
            DefaultColored = TargetEffect.colored.value;
            DefaultIntensity = TargetEffect.intensity.value;
            DefaultSize = TargetEffect.size.value;
            DefaultLumContrib = TargetEffect.lumContrib.value;
        }

        protected override void Reset()
        {
            Intensity = new ConstantChannel(DefaultIntensity);
            Size = new ConstantChannel(DefaultSize);
            LumContrib = new ConstantChannel(DefaultLumContrib);
            TargetEffect.colored.overrideState = false;
            TargetEffect.intensity.overrideState = false;
            TargetEffect.size.overrideState = false;
            TargetEffect.lumContrib.overrideState = false;
            TargetEffect.colored.value = DefaultColored;
        }
    }
}