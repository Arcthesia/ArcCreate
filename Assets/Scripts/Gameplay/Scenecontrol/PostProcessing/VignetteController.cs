using System.Collections.Generic;
using ArcCreate.Utility.Lua;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class VignetteController : PostProcessingController<Vignette>
    {
        public ValueChannel ColorR { get; set; }

        public ValueChannel ColorG { get; set; }

        public ValueChannel ColorB { get; set; }

        public ValueChannel ColorA { get; set; }

        public ValueChannel ColorH { get; set; }

        public ValueChannel ColorS { get; set; }

        public ValueChannel ColorV { get; set; }

        public ValueChannel CenterX { get; set; }

        public ValueChannel CenterY { get; set; }

        public ValueChannel Intensity { get; set; }

        public ValueChannel Smoothness { get; set; }

        public ValueChannel Roundness { get; set; }

        public float DefaultColorR { get; set; }

        public float DefaultColorG { get; set; }

        public float DefaultColorB { get; set; }

        public float DefaultColorA { get; set; }

        public float DefaultColorH { get; set; }

        public float DefaultColorS { get; set; }

        public float DefaultColorV { get; set; }

        public float DefaultCenterX { get; set; }

        public float DefaultCenterY { get; set; }

        public float DefaultIntensity { get; set; }

        public float DefaultSmoothness { get; set; }

        public float DefaultRoundness { get; set; }

        public bool DefaultRounded { get; set; }

        public override void EnableEffect(string[] effects)
        {
            TargetEffect.enabled.Override(true);
            foreach (string effect in effects)
            {
                switch (effect.ToLower())
                {
                    case "color": TargetEffect.color.overrideState = true; break;
                    case "center": TargetEffect.center.overrideState = true; break;
                    case "intensity": TargetEffect.intensity.overrideState = true; break;
                    case "smoothness": TargetEffect.smoothness.overrideState = true; break;
                    case "roundness": TargetEffect.roundness.overrideState = true; break;
                }
            }
        }

        public void SetRounded(bool rounded)
        {
            TargetEffect.rounded.Override(rounded);
        }

        public override void UpdateController(int timing)
        {
            RGBA c = new RGBA(ColorR.ValueAt(timing), ColorG.ValueAt(timing), ColorB.ValueAt(timing), ColorA.ValueAt(timing));
            HSVA modify = new HSVA(ColorH.ValueAt(timing), ColorS.ValueAt(timing), ColorV.ValueAt(timing), 1);

            HSVA hsva = Convert.RGBAToHSVA(c);
            hsva.H = (hsva.H + modify.H) % 360;
            hsva.S = UnityEngine.Mathf.Clamp(hsva.S + modify.S, 0, 1);
            hsva.V = UnityEngine.Mathf.Clamp(hsva.V + modify.V, 0, 1);
            TargetEffect.color.value = Convert.HSVAToRGBA(hsva).ToColor();

            TargetEffect.center.value = new Vector2(CenterX.ValueAt(timing), CenterY.ValueAt(timing));
            TargetEffect.intensity.value = Intensity.ValueAt(timing);
            TargetEffect.smoothness.value = Smoothness.ValueAt(timing);
            TargetEffect.roundness.value = Roundness.ValueAt(timing);
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                TargetEffect.enabled.value,
                TargetEffect.color.overrideState,
                TargetEffect.center.overrideState,
                TargetEffect.intensity.overrideState,
                TargetEffect.smoothness.overrideState,
                TargetEffect.roundness.overrideState,
                serialization.AddUnitAndGetId(ColorR),
                serialization.AddUnitAndGetId(ColorG),
                serialization.AddUnitAndGetId(ColorB),
                serialization.AddUnitAndGetId(ColorA),
                serialization.AddUnitAndGetId(ColorH),
                serialization.AddUnitAndGetId(ColorS),
                serialization.AddUnitAndGetId(ColorV),
                serialization.AddUnitAndGetId(CenterX),
                serialization.AddUnitAndGetId(CenterY),
                serialization.AddUnitAndGetId(Intensity),
                serialization.AddUnitAndGetId(Smoothness),
                serialization.AddUnitAndGetId(Roundness),
                TargetEffect.rounded.value,
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int offset = 0;
            TargetEffect.enabled.Override((bool)properties[offset++]);
            TargetEffect.color.overrideState = (bool)properties[offset++];
            TargetEffect.center.overrideState = (bool)properties[offset++];
            TargetEffect.intensity.overrideState = (bool)properties[offset++];
            TargetEffect.smoothness.overrideState = (bool)properties[offset++];
            TargetEffect.roundness.overrideState = (bool)properties[offset++];
            ColorR = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorG = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorB = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorA = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorH = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorS = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorV = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            CenterX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            CenterY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Intensity = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Smoothness = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Roundness = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            TargetEffect.rounded.Override((bool)properties[offset++]);
        }

        protected override void SetupDefault()
        {
            DefaultColorR = TargetEffect.color.value.r * 255f;
            DefaultColorG = TargetEffect.color.value.g * 255f;
            DefaultColorB = TargetEffect.color.value.b * 255f;
            DefaultColorA = TargetEffect.color.value.a * 255f;
            DefaultColorH = 0;
            DefaultColorS = 1;
            DefaultColorV = 1;
            DefaultCenterX = TargetEffect.center.value.x;
            DefaultCenterY = TargetEffect.center.value.y;
            DefaultIntensity = TargetEffect.intensity.value;
            DefaultSmoothness = TargetEffect.smoothness.value;
            DefaultRoundness = TargetEffect.roundness.value;
            DefaultRounded = TargetEffect.rounded.value;
        }

        protected override void Reset()
        {
            ColorR = new ConstantChannel(DefaultColorR);
            ColorG = new ConstantChannel(DefaultColorG);
            ColorB = new ConstantChannel(DefaultColorB);
            ColorA = new ConstantChannel(DefaultColorA);
            ColorH = new ConstantChannel(DefaultColorH);
            ColorS = new ConstantChannel(DefaultColorS);
            ColorV = new ConstantChannel(DefaultColorV);
            CenterX = new ConstantChannel(DefaultCenterX);
            CenterY = new ConstantChannel(DefaultCenterY);
            Intensity = new ConstantChannel(DefaultIntensity);
            Smoothness = new ConstantChannel(DefaultSmoothness);
            Roundness = new ConstantChannel(DefaultRoundness);
            TargetEffect.color.overrideState = false;
            TargetEffect.center.overrideState = false;
            TargetEffect.intensity.overrideState = false;
            TargetEffect.smoothness.overrideState = false;
            TargetEffect.roundness.overrideState = false;
            TargetEffect.rounded.overrideState = false;
            TargetEffect.rounded.value = DefaultRounded;
        }
    }
}