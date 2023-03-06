using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ChromaticAberrationController : PostProcessingController<ChromaticAberration>
    {
        public ValueChannel Intensity { get; set; }

        public float DefaultIntensity { get; set; }

        public bool DefaultFastMode { get; set; }

        public override void EnableEffect(string[] effects)
        {
            TargetEffect.enabled.Override(true);
            foreach (string effect in effects)
            {
                switch (effect.ToLower())
                {
                    case "intensity": TargetEffect.intensity.overrideState = true; break;
                }
            }
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                TargetEffect.enabled.value,
                TargetEffect.intensity.overrideState,
                serialization.AddUnitAndGetId(Intensity),
                TargetEffect.fastMode.value,
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int offset = 0;
            TargetEffect.enabled.Override((bool)properties[offset++]);
            TargetEffect.intensity.overrideState = (bool)properties[offset++];
            Intensity = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            TargetEffect.fastMode.Override((bool)properties[offset++]);
        }

        public ChromaticAberrationController SetFastMode(bool fastMode)
        {
            TargetEffect.fastMode.Override(fastMode);
            return this;
        }

        public override void UpdateController(int timing)
        {
            TargetEffect.intensity.value = Intensity.ValueAt(timing);
        }

        protected override void Reset()
        {
            Intensity = new ConstantChannel(DefaultIntensity);
            TargetEffect.intensity.overrideState = false;
            TargetEffect.fastMode.overrideState = false;
            TargetEffect.fastMode.value = DefaultFastMode;
        }

        protected override void SetupDefault()
        {
            DefaultIntensity = TargetEffect.intensity.value;
            DefaultFastMode = TargetEffect.fastMode.value;
        }
    }
}