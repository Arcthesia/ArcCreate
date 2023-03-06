using System.Collections.Generic;
using ArcCreate.Utilities.Lua;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ColorGradingController : PostProcessingController<ColorGrading>
    {
        public ValueChannel Temperature { get; set; }

        public ValueChannel Tint { get; set; }

        public ValueChannel ColorR { get; set; }

        public ValueChannel ColorG { get; set; }

        public ValueChannel ColorB { get; set; }

        public ValueChannel ColorA { get; set; }

        public ValueChannel ColorH { get; set; }

        public ValueChannel ColorS { get; set; }

        public ValueChannel ColorV { get; set; }

        public ValueChannel Contrast { get; set; }

        public ValueChannel MixerRedOutRedIn { get; set; }

        public ValueChannel MixerRedOutBlueIn { get; set; }

        public ValueChannel MixerRedOutGreenIn { get; set; }

        public ValueChannel MixerGreenOutRedIn { get; set; }

        public ValueChannel MixerGreenOutBlueIn { get; set; }

        public ValueChannel MixerGreenOutGreenIn { get; set; }

        public ValueChannel MixerBlueOutRedIn { get; set; }

        public ValueChannel MixerBlueOutBlueIn { get; set; }

        public ValueChannel MixerBlueOutGreenIn { get; set; }

        public ValueChannel LiftX { get; set; }

        public ValueChannel LiftY { get; set; }

        public ValueChannel LiftZ { get; set; }

        public ValueChannel LiftW { get; set; }

        public ValueChannel GammaX { get; set; }

        public ValueChannel GammaY { get; set; }

        public ValueChannel GammaZ { get; set; }

        public ValueChannel GammaW { get; set; }

        public ValueChannel GainX { get; set; }

        public ValueChannel GainY { get; set; }

        public ValueChannel GainZ { get; set; }

        public ValueChannel GainW { get; set; }

        public float DefaultTemperature { get; set; }

        public float DefaultTint { get; set; }

        public float DefaultColorR { get; set; }

        public float DefaultColorG { get; set; }

        public float DefaultColorB { get; set; }

        public float DefaultColorA { get; set; }

        public float DefaultColorH { get; set; }

        public float DefaultColorS { get; set; }

        public float DefaultColorV { get; set; }

        public float DefaultContrast { get; set; }

        public float DefaultMixerRedOutRedIn { get; set; }

        public float DefaultMixerRedOutBlueIn { get; set; }

        public float DefaultMixerRedOutGreenIn { get; set; }

        public float DefaultMixerGreenOutRedIn { get; set; }

        public float DefaultMixerGreenOutBlueIn { get; set; }

        public float DefaultMixerGreenOutGreenIn { get; set; }

        public float DefaultMixerBlueOutRedIn { get; set; }

        public float DefaultMixerBlueOutBlueIn { get; set; }

        public float DefaultMixerBlueOutGreenIn { get; set; }

        public float DefaultLiftX { get; set; }

        public float DefaultLiftY { get; set; }

        public float DefaultLiftZ { get; set; }

        public float DefaultLiftW { get; set; }

        public float DefaultGammaX { get; set; }

        public float DefaultGammaY { get; set; }

        public float DefaultGammaZ { get; set; }

        public float DefaultGammaW { get; set; }

        public float DefaultGainX { get; set; }

        public float DefaultGainY { get; set; }

        public float DefaultGainZ { get; set; }

        public float DefaultGainW { get; set; }

        public override void EnableEffect(string[] effects)
        {
            TargetEffect.enabled.Override(true);
            foreach (string effect in effects)
            {
                switch (effect.ToLower())
                {
                    case "temperature": TargetEffect.temperature.overrideState = true; break;
                    case "tint": TargetEffect.tint.overrideState = true; break;
                    case "colorfilter": TargetEffect.colorFilter.overrideState = true; break;
                    case "hueshift": TargetEffect.hueShift.overrideState = true; break;
                    case "saturation": TargetEffect.saturation.overrideState = true; break;
                    case "brightness": TargetEffect.brightness.overrideState = true; break;
                    case "constrast": TargetEffect.contrast.overrideState = true; break;
                    case "mixerredoutredin": TargetEffect.mixerRedOutRedIn.overrideState = true; break;
                    case "mixerredoutbluein": TargetEffect.mixerRedOutBlueIn.overrideState = true; break;
                    case "mixerredoutgreenin": TargetEffect.mixerRedOutGreenIn.overrideState = true; break;
                    case "mixergreenoutredin": TargetEffect.mixerGreenOutRedIn.overrideState = true; break;
                    case "mixergreenoutbluein": TargetEffect.mixerGreenOutBlueIn.overrideState = true; break;
                    case "mixergreenoutgreenin": TargetEffect.mixerGreenOutGreenIn.overrideState = true; break;
                    case "mixerblueoutredin": TargetEffect.mixerBlueOutRedIn.overrideState = true; break;
                    case "mixerblueoutbluein": TargetEffect.mixerBlueOutBlueIn.overrideState = true; break;
                    case "mixerblueoutgreenin": TargetEffect.mixerBlueOutGreenIn.overrideState = true; break;
                    case "lift": TargetEffect.lift.overrideState = true; break;
                    case "gamma": TargetEffect.gamma.overrideState = true; break;
                    case "gain": TargetEffect.gain.overrideState = true; break;
                }
            }
        }

        public override void UpdateController(int timing)
        {
            RGBA c = new RGBA(ColorR.ValueAt(timing), ColorG.ValueAt(timing), ColorB.ValueAt(timing), ColorA.ValueAt(timing));
            Color color = c.ToColor();

            Vector4 lift = new Vector4(
                LiftX.ValueAt(timing),
                LiftY.ValueAt(timing),
                LiftZ.ValueAt(timing),
                LiftW.ValueAt(timing));
            Vector4 gamma = new Vector4(
                GammaX.ValueAt(timing),
                GammaY.ValueAt(timing),
                GammaZ.ValueAt(timing),
                GammaW.ValueAt(timing));
            Vector4 gain = new Vector4(
                GainX.ValueAt(timing),
                GainY.ValueAt(timing),
                GainZ.ValueAt(timing),
                GainW.ValueAt(timing));

            TargetEffect.temperature.value = Temperature.ValueAt(timing);
            TargetEffect.tint.value = Tint.ValueAt(timing);
            TargetEffect.colorFilter.value = color;
            TargetEffect.hueShift.value = ColorH.ValueAt(timing) - 180;
            TargetEffect.saturation.value = ColorS.ValueAt(timing) * 100;
            TargetEffect.brightness.value = ColorV.ValueAt(timing) * 100;
            TargetEffect.mixerRedOutRedIn.value = MixerRedOutRedIn.ValueAt(timing);
            TargetEffect.mixerRedOutBlueIn.value = MixerRedOutBlueIn.ValueAt(timing);
            TargetEffect.mixerRedOutGreenIn.value = MixerRedOutGreenIn.ValueAt(timing);
            TargetEffect.mixerGreenOutRedIn.value = MixerGreenOutRedIn.ValueAt(timing);
            TargetEffect.mixerGreenOutBlueIn.value = MixerGreenOutBlueIn.ValueAt(timing);
            TargetEffect.mixerGreenOutGreenIn.value = MixerGreenOutGreenIn.ValueAt(timing);
            TargetEffect.mixerBlueOutRedIn.value = MixerBlueOutRedIn.ValueAt(timing);
            TargetEffect.mixerBlueOutBlueIn.value = MixerBlueOutBlueIn.ValueAt(timing);
            TargetEffect.mixerBlueOutGreenIn.value = MixerBlueOutGreenIn.ValueAt(timing);
            TargetEffect.lift.value = lift;
            TargetEffect.gamma.value = gamma;
            TargetEffect.gain.value = gain;
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                TargetEffect.enabled.value,
                TargetEffect.temperature.overrideState,
                TargetEffect.tint.overrideState,
                TargetEffect.colorFilter.overrideState,
                TargetEffect.hueShift.overrideState,
                TargetEffect.saturation.overrideState,
                TargetEffect.brightness.overrideState,
                TargetEffect.contrast.overrideState,
                TargetEffect.mixerRedOutRedIn.overrideState,
                TargetEffect.mixerRedOutBlueIn.overrideState,
                TargetEffect.mixerRedOutGreenIn.overrideState,
                TargetEffect.mixerGreenOutRedIn.overrideState,
                TargetEffect.mixerGreenOutBlueIn.overrideState,
                TargetEffect.mixerGreenOutGreenIn.overrideState,
                TargetEffect.mixerBlueOutRedIn.overrideState,
                TargetEffect.mixerBlueOutBlueIn.overrideState,
                TargetEffect.mixerBlueOutGreenIn.overrideState,
                TargetEffect.lift.overrideState,
                TargetEffect.gamma.overrideState,
                TargetEffect.gain.overrideState,
                serialization.AddUnitAndGetId(Temperature),
                serialization.AddUnitAndGetId(Tint),
                serialization.AddUnitAndGetId(ColorR),
                serialization.AddUnitAndGetId(ColorG),
                serialization.AddUnitAndGetId(ColorB),
                serialization.AddUnitAndGetId(ColorA),
                serialization.AddUnitAndGetId(ColorH),
                serialization.AddUnitAndGetId(ColorS),
                serialization.AddUnitAndGetId(ColorV),
                serialization.AddUnitAndGetId(Contrast),
                serialization.AddUnitAndGetId(MixerRedOutRedIn),
                serialization.AddUnitAndGetId(MixerRedOutBlueIn),
                serialization.AddUnitAndGetId(MixerRedOutGreenIn),
                serialization.AddUnitAndGetId(MixerGreenOutRedIn),
                serialization.AddUnitAndGetId(MixerGreenOutBlueIn),
                serialization.AddUnitAndGetId(MixerGreenOutGreenIn),
                serialization.AddUnitAndGetId(MixerBlueOutRedIn),
                serialization.AddUnitAndGetId(MixerBlueOutBlueIn),
                serialization.AddUnitAndGetId(MixerBlueOutGreenIn),
                serialization.AddUnitAndGetId(LiftX),
                serialization.AddUnitAndGetId(LiftY),
                serialization.AddUnitAndGetId(LiftZ),
                serialization.AddUnitAndGetId(LiftW),
                serialization.AddUnitAndGetId(GammaX),
                serialization.AddUnitAndGetId(GammaY),
                serialization.AddUnitAndGetId(GammaZ),
                serialization.AddUnitAndGetId(GammaW),
                serialization.AddUnitAndGetId(GainX),
                serialization.AddUnitAndGetId(GainY),
                serialization.AddUnitAndGetId(GainZ),
                serialization.AddUnitAndGetId(GainW),
            };
        }

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int offset = 0;
            TargetEffect.enabled.Override((bool)properties[offset++]);
            TargetEffect.temperature.overrideState = (bool)properties[offset++];
            TargetEffect.tint.overrideState = (bool)properties[offset++];
            TargetEffect.colorFilter.overrideState = (bool)properties[offset++];
            TargetEffect.hueShift.overrideState = (bool)properties[offset++];
            TargetEffect.saturation.overrideState = (bool)properties[offset++];
            TargetEffect.brightness.overrideState = (bool)properties[offset++];
            TargetEffect.contrast.overrideState = (bool)properties[offset++];
            TargetEffect.mixerRedOutRedIn.overrideState = (bool)properties[offset++];
            TargetEffect.mixerRedOutBlueIn.overrideState = (bool)properties[offset++];
            TargetEffect.mixerRedOutGreenIn.overrideState = (bool)properties[offset++];
            TargetEffect.mixerGreenOutRedIn.overrideState = (bool)properties[offset++];
            TargetEffect.mixerGreenOutBlueIn.overrideState = (bool)properties[offset++];
            TargetEffect.mixerGreenOutGreenIn.overrideState = (bool)properties[offset++];
            TargetEffect.mixerBlueOutRedIn.overrideState = (bool)properties[offset++];
            TargetEffect.mixerBlueOutBlueIn.overrideState = (bool)properties[offset++];
            TargetEffect.mixerBlueOutGreenIn.overrideState = (bool)properties[offset++];
            TargetEffect.lift.overrideState = (bool)properties[offset++];
            TargetEffect.gamma.overrideState = (bool)properties[offset++];
            TargetEffect.gain.overrideState = (bool)properties[offset++];
            Temperature = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Tint = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorR = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorG = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorB = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorA = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorH = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorS = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            ColorV = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            Contrast = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MixerRedOutRedIn = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MixerRedOutBlueIn = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MixerRedOutGreenIn = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MixerGreenOutRedIn = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MixerGreenOutBlueIn = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MixerGreenOutGreenIn = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MixerBlueOutRedIn = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MixerBlueOutBlueIn = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            MixerBlueOutGreenIn = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            LiftX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            LiftY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            LiftZ = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            LiftW = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            GammaX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            GammaY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            GammaZ = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            GammaW = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            GainX = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            GainY = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            GainZ = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            GainW = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
        }

        protected override void SetupDefault()
        {
            DefaultTemperature = TargetEffect.temperature.value;
            DefaultTint = TargetEffect.tint.value;
            DefaultColorR = TargetEffect.colorFilter.value.r * 255;
            DefaultColorG = TargetEffect.colorFilter.value.g * 255;
            DefaultColorB = TargetEffect.colorFilter.value.b * 255;
            DefaultColorA = TargetEffect.colorFilter.value.a * 255;
            DefaultColorH = TargetEffect.hueShift.value;
            DefaultColorS = TargetEffect.saturation.value;
            DefaultColorV = TargetEffect.brightness.value;
            DefaultContrast = TargetEffect.contrast.value;
            DefaultMixerRedOutRedIn = TargetEffect.mixerRedOutRedIn.value;
            DefaultMixerRedOutBlueIn = TargetEffect.mixerRedOutBlueIn.value;
            DefaultMixerRedOutGreenIn = TargetEffect.mixerRedOutGreenIn.value;
            DefaultMixerGreenOutRedIn = TargetEffect.mixerGreenOutRedIn.value;
            DefaultMixerGreenOutBlueIn = TargetEffect.mixerGreenOutBlueIn.value;
            DefaultMixerGreenOutGreenIn = TargetEffect.mixerGreenOutGreenIn.value;
            DefaultMixerBlueOutRedIn = TargetEffect.mixerBlueOutRedIn.value;
            DefaultMixerBlueOutBlueIn = TargetEffect.mixerBlueOutBlueIn.value;
            DefaultMixerBlueOutGreenIn = TargetEffect.mixerBlueOutGreenIn.value;
            DefaultLiftX = TargetEffect.lift.value.x;
            DefaultLiftY = TargetEffect.lift.value.y;
            DefaultLiftZ = TargetEffect.lift.value.z;
            DefaultLiftW = TargetEffect.lift.value.w;
            DefaultGammaX = TargetEffect.gamma.value.x;
            DefaultGammaY = TargetEffect.gamma.value.y;
            DefaultGammaZ = TargetEffect.gamma.value.z;
            DefaultGammaW = TargetEffect.gamma.value.w;
            DefaultGainX = TargetEffect.gain.value.x;
            DefaultGainY = TargetEffect.gain.value.y;
            DefaultGainZ = TargetEffect.gain.value.z;
            DefaultGainW = TargetEffect.gain.value.w;
        }

        protected override void Reset()
        {
            Temperature = new ConstantChannel(DefaultTemperature);
            Tint = new ConstantChannel(DefaultTint);
            ColorR = new ConstantChannel(DefaultColorR);
            ColorG = new ConstantChannel(DefaultColorG);
            ColorB = new ConstantChannel(DefaultColorB);
            ColorA = new ConstantChannel(DefaultColorA);
            ColorH = new ConstantChannel(DefaultColorH);
            ColorS = new ConstantChannel(DefaultColorS);
            ColorV = new ConstantChannel(DefaultColorV);
            Contrast = new ConstantChannel(DefaultContrast);
            MixerRedOutRedIn = new ConstantChannel(DefaultMixerRedOutRedIn);
            MixerRedOutBlueIn = new ConstantChannel(DefaultMixerRedOutBlueIn);
            MixerRedOutGreenIn = new ConstantChannel(DefaultMixerRedOutGreenIn);
            MixerGreenOutRedIn = new ConstantChannel(DefaultMixerGreenOutRedIn);
            MixerGreenOutBlueIn = new ConstantChannel(DefaultMixerGreenOutBlueIn);
            MixerGreenOutGreenIn = new ConstantChannel(DefaultMixerGreenOutGreenIn);
            MixerBlueOutRedIn = new ConstantChannel(DefaultMixerBlueOutRedIn);
            MixerBlueOutBlueIn = new ConstantChannel(DefaultMixerBlueOutBlueIn);
            MixerBlueOutGreenIn = new ConstantChannel(DefaultMixerBlueOutGreenIn);
            LiftX = new ConstantChannel(DefaultLiftX);
            LiftY = new ConstantChannel(DefaultLiftY);
            LiftZ = new ConstantChannel(DefaultLiftZ);
            LiftW = new ConstantChannel(DefaultLiftW);
            GammaX = new ConstantChannel(DefaultGammaX);
            GammaY = new ConstantChannel(DefaultGammaY);
            GammaZ = new ConstantChannel(DefaultGammaZ);
            GammaW = new ConstantChannel(DefaultGammaW);
            GainX = new ConstantChannel(DefaultGainX);
            GainY = new ConstantChannel(DefaultGainY);
            GainZ = new ConstantChannel(DefaultGainZ);
            GainW = new ConstantChannel(DefaultGainW);
            TargetEffect.temperature.overrideState = false;
            TargetEffect.tint.overrideState = false;
            TargetEffect.colorFilter.overrideState = false;
            TargetEffect.hueShift.overrideState = false;
            TargetEffect.saturation.overrideState = false;
            TargetEffect.brightness.overrideState = false;
            TargetEffect.contrast.overrideState = false;
            TargetEffect.mixerRedOutRedIn.overrideState = false;
            TargetEffect.mixerRedOutBlueIn.overrideState = false;
            TargetEffect.mixerRedOutGreenIn.overrideState = false;
            TargetEffect.mixerGreenOutRedIn.overrideState = false;
            TargetEffect.mixerGreenOutBlueIn.overrideState = false;
            TargetEffect.mixerGreenOutGreenIn.overrideState = false;
            TargetEffect.mixerBlueOutRedIn.overrideState = false;
            TargetEffect.mixerBlueOutBlueIn.overrideState = false;
            TargetEffect.mixerBlueOutGreenIn.overrideState = false;
            TargetEffect.lift.overrideState = false;
            TargetEffect.gamma.overrideState = false;
            TargetEffect.gain.overrideState = false;
            TargetEffect.masterCurve.overrideState = false;
            TargetEffect.redCurve.overrideState = false;
            TargetEffect.blueCurve.overrideState = false;
            TargetEffect.greenCurve.overrideState = false;
            TargetEffect.hueVsHueCurve.overrideState = false;
            TargetEffect.hueVsSatCurve.overrideState = false;
            TargetEffect.lumVsSatCurve.overrideState = false;
            TargetEffect.satVsSatCurve.overrideState = false;
        }
    }
}