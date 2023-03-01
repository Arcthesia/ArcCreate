using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    public class ValueChannelBuilder
    {
        public static ValueChannel Keyframe() => new KeyChannel();

        public static ValueChannel FFT(int freqBandMin, int freqBandMax, float min, float max, float smoothness, float scalar)
            => new FFTChannel(freqBandMin, freqBandMax, min, max, smoothness, scalar);

        public static ValueChannel Clamp(ValueChannel a, ValueChannel b, ValueChannel c)
            => new ClampChannel(a, b, c);

        public static ValueChannel Condition(ValueChannel control, ValueChannel threshold, ValueChannel ifAbove, ValueChannel ifEqual, ValueChannel ifBelow)
            => new ConditionalChannel(control, threshold, ifAbove, ifEqual, ifBelow);

        public static ValueChannel Constant(float val)
            => new ConstantChannel(val);

        public static ValueChannel Cos(ValueChannel period, ValueChannel min, ValueChannel max, ValueChannel offset)
            => new CosChannel(period, min, max, offset);

        public static ValueChannel Exp(ValueChannel num, ValueChannel exp)
            => new ExpChannel(num, exp);

        public static ValueChannel Max(ValueChannel a, ValueChannel b)
            => new MaxChannel(a, b);

        public static ValueChannel Min(ValueChannel a, ValueChannel b)
            => new MinChannel(a, b);

        public static ValueChannel Noise(ValueChannel frequency, ValueChannel min, ValueChannel max, ValueChannel offset, ValueChannel octave)
            => new NoiseChannel(frequency, min, max, offset, octave);

        public static ValueChannel Random(int seed, ValueChannel min, ValueChannel max)
            => new RandomChannel(seed, min, max);

        public static ValueChannel Saw(string easing, ValueChannel period, ValueChannel min, ValueChannel max, ValueChannel offset)
            => new SawChannel(easing, period, min, max, offset);

        public static ValueChannel Sine(ValueChannel period, ValueChannel min, ValueChannel max, ValueChannel offset)
            => new SineChannel(period, min, max, offset);
    }
}