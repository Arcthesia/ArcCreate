using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class FFTChannel : ValueChannel
    {
        private int freqBandMin;
        private int freqBandMax;
        private float min;
        private float max;
        private float smoothness;
        private float scalar;
        private readonly AudioSource source;

        private int cachedTiming = int.MinValue;

        public FFTChannel()
        {
            source = Services.Audio.AudioSource;
        }

        public FFTChannel(int freqBandMin, int freqBandMax, float min, float max, float smoothness, float scalar)
        {
            source = Services.Audio.AudioSource;
            if (freqBandMin > freqBandMax)
            {
                (freqBandMin, freqBandMax) = (freqBandMax, freqBandMin);
            }

            this.freqBandMin = Mathf.Clamp(freqBandMin, 0, Spectrum.Length - 1);
            this.freqBandMax = Mathf.Clamp(freqBandMax, 0, Spectrum.Length - 1);
            this.min = min;
            this.max = max;
            this.smoothness = Mathf.Clamp(smoothness, 0, 1);

            // Don't ask what this is. I don't know either
            this.scalar = scalar * Mathf.Pow(Spectrum.Length, 0.58f) * 4;
        }

        public static float[] Spectrum { get; set; } = new float[256];

        public static float[] SpectrumSmoothed { get; set; } = new float[256];

        public static FFTWindow Window { get; set; } = FFTWindow.Triangle;

        public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            freqBandMin = (int)(double)properties[0];
            freqBandMax = (int)(double)properties[1];
            min = (float)(double)properties[2];
            max = (float)(double)properties[3];
            smoothness = (float)(double)properties[4];
            scalar = (float)(double)properties[5];
        }

        public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            List<object> properties = new List<object>
            {
                freqBandMin,
                freqBandMax,
                min,
                max,
                smoothness,
                scalar,
            };

            return properties;
        }

        public override float ValueAt(int timing)
        {
            if (timing != cachedTiming)
            {
                source.GetSpectrumData(Spectrum, 0, Window);
                for (int i = 0; i < Spectrum.Length; i++)
                {
                    SpectrumSmoothed[i] = (Spectrum[i] * (1 - smoothness)) + (SpectrumSmoothed[i] * smoothness);
                    Spectrum[i] = SpectrumSmoothed[i];
                }

                cachedTiming = timing;
            }

            float res = 0;
            for (int i = freqBandMin; i <= freqBandMax; i++)
            {
                res += SpectrumSmoothed[i] * scalar / source.volume;
            }

            return min + ((max - min) * res / (freqBandMax - freqBandMin + 1));
        }

        protected override IEnumerable<ValueChannel> GetChildrenChannels()
        {
            yield break;
        }
    }
}