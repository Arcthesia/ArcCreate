using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class DepthOfFieldController : PostProcessingController<DepthOfField>
    {
        // Depth of field blurs everything in the scene anyway
        public ValueChannel FocusDistance { get; set; }

        public ValueChannel Aperture { get; set; }

        public ValueChannel FocalLength { get; set; }

        public float DefaultFocusDistance { get; set; }

        public float DefaultAperture { get; set; }

        public float DefaultFocalLength { get; set; }

        public override void EnableEffect(string[] effects)
        {
            TargetEffect.enabled.Override(true);
            foreach (string effect in effects)
            {
                switch (effect.ToLower())
                {
                    case "focusdistance": TargetEffect.focusDistance.overrideState = true; break;
                    case "aperture": TargetEffect.focusDistance.overrideState = true; break;
                    case "focallength": TargetEffect.focalLength.overrideState = true; break;
                }
            }
        }

        public override void UpdateController(int timing)
        {
            TargetEffect.focusDistance.value = FocusDistance.ValueAt(timing);
            TargetEffect.focalLength.value = FocalLength.ValueAt(timing);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                TargetEffect.enabled.value,
                TargetEffect.focusDistance.overrideState,
                TargetEffect.focusDistance.overrideState,
                TargetEffect.focalLength.overrideState,
                serialization.AddUnitAndGetId(FocusDistance),
                serialization.AddUnitAndGetId(Aperture),
                serialization.AddUnitAndGetId(FocalLength),
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int offset = 0;
            TargetEffect.enabled.Override((bool)properties[offset++]);
            TargetEffect.focusDistance.overrideState = (bool)properties[offset++];
            TargetEffect.focusDistance.overrideState = (bool)properties[offset++];
            TargetEffect.focalLength.overrideState = (bool)properties[offset++];
            FocusDistance = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Aperture = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            FocalLength = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
        }

        protected override void SetupDefault()
        {
            DefaultFocusDistance = TargetEffect.focusDistance.value;
            DefaultAperture = TargetEffect.aperture.value;
            DefaultFocalLength = TargetEffect.focalLength.value;
        }

        protected override void Reset()
        {
            FocusDistance = new ConstantChannel(DefaultFocusDistance);
            Aperture = new ConstantChannel(DefaultAperture);
            FocalLength = new ConstantChannel(DefaultFocalLength);
            TargetEffect.focusDistance.overrideState = false;
            TargetEffect.aperture.overrideState = false;
            TargetEffect.focalLength.overrideState = false;
        }
    }
}