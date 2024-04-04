using System.Collections.Generic;
using ArcCreate.Utility.Lua;
using MoonSharp.Interpreter;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class BloomController : PostProcessingController<Bloom>
    {
        public ValueChannel Intensity { get; set; }

        public ValueChannel Threshold { get; set; }

        public ValueChannel SoftKnee { get; set; }

        public ValueChannel Clamp { get; set; }

        public ValueChannel Diffusion { get; set; }

        public ValueChannel AnamorphicRatio { get; set; }

        public ValueChannel ColorR { get; set; }

        public ValueChannel ColorG { get; set; }

        public ValueChannel ColorB { get; set; }

        public ValueChannel ColorA { get; set; }

        public ValueChannel ColorH { get; set; }

        public ValueChannel ColorS { get; set; }

        public ValueChannel ColorV { get; set; }

        public float DefaultIntensity { get; set; }

        public float DefaultThreshold { get; set; }

        public float DefaultSoftKnee { get; set; }

        public float DefaultClamp { get; set; }

        public float DefaultDiffusion { get; set; }

        public float DefaultAnamorphicRatio { get; set; }

        public float DefaultColorR { get; set; }

        public float DefaultColorG { get; set; }

        public float DefaultColorB { get; set; }

        public float DefaultColorA { get; set; }

        public float DefaultColorH { get; set; }

        public float DefaultColorS { get; set; }

        public float DefaultColorV { get; set; }

        public bool DefaultFastMode { get; set; }

        public override void EnableEffect(string[] effects)
        {
            TargetEffect.enabled.Override(true);
            foreach (string effect in effects)
            {
                switch (effect.ToLower())
                {
                    case "intensity": TargetEffect.intensity.overrideState = true; break;
                    case "threshold": TargetEffect.threshold.overrideState = true; break;
                    case "softknee": TargetEffect.softKnee.overrideState = true; break;
                    case "clamp": TargetEffect.clamp.overrideState = true; break;
                    case "diffusion": TargetEffect.diffusion.overrideState = true; break;
                    case "anamorphicratio": TargetEffect.anamorphicRatio.overrideState = true; break;
                    case "color": TargetEffect.color.overrideState = true; break;
                }
            }
        }

        public BloomController SetFastMode(bool fastMode)
        {
            TargetEffect.fastMode.Override(fastMode);
            return this;
        }

        public override void UpdateController(int timing)
        {
            RGBA c = new RGBA(ColorR.ValueAt(timing), ColorG.ValueAt(timing), ColorB.ValueAt(timing), ColorA.ValueAt(timing));
            HSVA modify = new HSVA(ColorH.ValueAt(timing), ColorS.ValueAt(timing), ColorV.ValueAt(timing), 1);

            HSVA hsva = Convert.RGBAToHSVA(c);
            hsva.H = (hsva.H + modify.H) % 360;
            hsva.S = UnityEngine.Mathf.Clamp(hsva.S + modify.S, 0, 1);
            hsva.V = UnityEngine.Mathf.Clamp(hsva.S + modify.V, 0, 1);
            TargetEffect.color.value = Convert.HSVAToRGBA(hsva).ToColor();

            TargetEffect.intensity.value = Intensity.ValueAt(timing);
            TargetEffect.threshold.value = Threshold.ValueAt(timing);
            TargetEffect.softKnee.value = SoftKnee.ValueAt(timing);
            TargetEffect.clamp.value = Clamp.ValueAt(timing);
            TargetEffect.diffusion.value = Diffusion.ValueAt(timing);
            TargetEffect.anamorphicRatio.value = AnamorphicRatio.ValueAt(timing);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                TargetEffect.enabled.value,
                TargetEffect.intensity.overrideState,
                TargetEffect.threshold.overrideState,
                TargetEffect.softKnee.overrideState,
                TargetEffect.clamp.overrideState,
                TargetEffect.diffusion.overrideState,
                TargetEffect.anamorphicRatio.overrideState,
                TargetEffect.color.overrideState,
                serialization.AddUnitAndGetId(Intensity),
                serialization.AddUnitAndGetId(Threshold),
                serialization.AddUnitAndGetId(SoftKnee),
                serialization.AddUnitAndGetId(Clamp),
                serialization.AddUnitAndGetId(Diffusion),
                serialization.AddUnitAndGetId(AnamorphicRatio),
                serialization.AddUnitAndGetId(ColorR),
                serialization.AddUnitAndGetId(ColorG),
                serialization.AddUnitAndGetId(ColorB),
                serialization.AddUnitAndGetId(ColorA),
                serialization.AddUnitAndGetId(ColorH),
                serialization.AddUnitAndGetId(ColorS),
                serialization.AddUnitAndGetId(ColorV),
                TargetEffect.fastMode.value,
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int offset = 0;
            TargetEffect.enabled.Override((bool)properties[offset++] && !Settings.DisableAdvancedGraphics.Value);
            TargetEffect.intensity.overrideState = (bool)properties[offset++];
            TargetEffect.threshold.overrideState = (bool)properties[offset++];
            TargetEffect.softKnee.overrideState = (bool)properties[offset++];
            TargetEffect.clamp.overrideState = (bool)properties[offset++];
            TargetEffect.diffusion.overrideState = (bool)properties[offset++];
            TargetEffect.anamorphicRatio.overrideState = (bool)properties[offset++];
            TargetEffect.color.overrideState = (bool)properties[offset++];
            Intensity = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Threshold = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            SoftKnee = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Clamp = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Diffusion = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            AnamorphicRatio = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorR = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorG = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorB = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorA = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorH = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorS = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorV = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            TargetEffect.fastMode.Override((bool)properties[offset++]);
        }

        protected override void SetupDefault()
        {
            DefaultIntensity = TargetEffect.intensity.value;
            DefaultThreshold = TargetEffect.threshold.value;
            DefaultSoftKnee = TargetEffect.softKnee.value;
            DefaultClamp = TargetEffect.clamp.value;
            DefaultDiffusion = TargetEffect.diffusion.value;
            DefaultAnamorphicRatio = TargetEffect.anamorphicRatio.value;
            DefaultColorR = TargetEffect.color.value.r;
            DefaultColorG = TargetEffect.color.value.g;
            DefaultColorB = TargetEffect.color.value.b;
            DefaultColorA = TargetEffect.color.value.a;
            DefaultColorH = 0;
            DefaultColorS = 1;
            DefaultColorV = 1;
            DefaultFastMode = TargetEffect.fastMode.value;
        }

        protected override void Reset()
        {
            Intensity = new ConstantChannel(DefaultIntensity);
            Threshold = new ConstantChannel(DefaultThreshold);
            SoftKnee = new ConstantChannel(DefaultSoftKnee);
            Clamp = new ConstantChannel(DefaultClamp);
            Diffusion = new ConstantChannel(DefaultDiffusion);
            AnamorphicRatio = new ConstantChannel(DefaultAnamorphicRatio);
            ColorR = new ConstantChannel(DefaultColorR);
            ColorG = new ConstantChannel(DefaultColorG);
            ColorB = new ConstantChannel(DefaultColorB);
            ColorA = new ConstantChannel(DefaultColorA);
            ColorH = new ConstantChannel(DefaultColorH);
            ColorS = new ConstantChannel(DefaultColorS);
            ColorV = new ConstantChannel(DefaultColorV);
            TargetEffect.intensity.overrideState = false;
            TargetEffect.threshold.overrideState = false;
            TargetEffect.softKnee.overrideState = false;
            TargetEffect.clamp.overrideState = false;
            TargetEffect.diffusion.overrideState = false;
            TargetEffect.anamorphicRatio.overrideState = false;
            TargetEffect.color.overrideState = false;
            TargetEffect.fastMode.overrideState = false;
            TargetEffect.fastMode.value = DefaultFastMode;
        }
    }
}