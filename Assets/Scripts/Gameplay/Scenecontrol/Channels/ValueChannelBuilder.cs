using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyAlias("Channel")]
    [EmmyDoc("Class for creating different channels")]
    [EmmySingleton]
    public class ValueChannelBuilder
    {
        [EmmyDoc("Create a keyframe channel")]
        public static KeyChannel Keyframe() => new KeyChannel();

        [EmmyDoc("Create a fast fourier transform channel that reacts to the system's audio")]
        [EmmyAlias("FFT")]
        public static FFTChannel FFT(int freqBandMin, int freqBandMax, float min, float max, float smoothness, float scalar)
            => new FFTChannel(freqBandMin, freqBandMax, min, max, smoothness, scalar);

        [EmmyDoc("Create a channel that clamp between values")]
        public static ClampChannel Clamp(ValueChannel a, ValueChannel b, ValueChannel c)
            => new ClampChannel(a, b, c);

        [EmmyDoc("Create a conditional channel")]
        public static ConditionalChannel Condition(ValueChannel control, ValueChannel threshold, ValueChannel ifAbove, ValueChannel ifEqual, ValueChannel ifBelow)
            => new ConditionalChannel(control, threshold, ifAbove, ifEqual, ifBelow);

        [EmmyDoc("Create a constant channel")]
        public static ConstantChannel Constant(float val)
            => new ConstantChannel(val);

        [EmmyDoc("Create a periodic cosine channel")]
        public static CosChannel Cos(ValueChannel period, ValueChannel min, ValueChannel max, ValueChannel offset)
            => new CosChannel(period, min, max, offset);

        [EmmyDoc("Create an exponential channel")]
        public static ExpChannel Exp(ValueChannel num, ValueChannel exp)
            => new ExpChannel(num, exp);

        [EmmyDoc("Create a channel that chooses max value between two channels")]
        public static MaxChannel Max(ValueChannel a, ValueChannel b)
            => new MaxChannel(a, b);

        [EmmyDoc("Create a channel that chooses min value between two channels")]
        public static MinChannel Min(ValueChannel a, ValueChannel b)
            => new MinChannel(a, b);

        [EmmyDoc("Create a perlin noise channel")]
        public static NoiseChannel Noise(ValueChannel frequency, ValueChannel min, ValueChannel max, ValueChannel offset, ValueChannel octave)
            => new NoiseChannel(frequency, min, max, offset, octave);

        [EmmyDoc("Create a random channel")]
        public static RandomChannel Random(int seed, ValueChannel min, ValueChannel max)
            => new RandomChannel(seed, min, max);

        [EmmyDoc("Create a saw channel that loop between values")]
        public static SawChannel Saw(
#pragma warning disable
            [EmmyChoice(
                "linear", "l", "inconstant", "inconst", "cnsti",
                "outconstant", "outconst", "cnsto", "inoutconstant", "inoutconst",
                "cnstb", "insine", "si", "outsine", "so",
                "inoutsine", "b", "inquadratic", "inquad", "2i",
                "outquadratic", "outquad", "2o", "inoutquadratic", "inoutquad",
                "2b", "incubic", "3i", "outcubic", "outcube",
                "3o", "inoutcubic", "inoutcube", "3b", "inquartic",
                "inquart", "4i", "outquartic", "outquart", "4o",
                "inoutquartic", "inoutquart", "4b", "inquintic", "inquint",
                "5i", "outquintic", "outquint", "5o", "inoutquintic",
                "inoutquint", "5b", "inexponential", "inexpo", "exi",
                "outexponential", "outexpo", "exo", "inoutexponential", "inoutexpo",
                "exb", "incircle", "incirc", "ci", "outcircle",
                "outcirc", "co", "inoutcircle", "inoutcirc", "cb",
                "inback", "bki", "outback", "bko", "inoutback",
                "bkb", "inelastic", "eli", "outelastic", "elo",
                "inoutelastic", "elb", "inbounce", "bni", "outbounce",
                "bno", "inoutbounce", "bnb")]
            string easing,
#pragma warning restore
            ValueChannel period,
            ValueChannel min,
            ValueChannel max,
            ValueChannel offset)
            => new SawChannel(easing, period, min, max, offset);

        [EmmyDoc("Create a periodic sine channel")]
        public static SineChannel Sine(ValueChannel period, ValueChannel min, ValueChannel max, ValueChannel offset)
            => new SineChannel(period, min, max, offset);

        [EmmyDoc("Create a pure sine channel")]
        public static PureSineChannel Sine(ValueChannel input)
            => new PureSineChannel(input);

        [EmmyDoc("Create a pure cosine channel")]
        public static PureCosChannel Cos(ValueChannel input)
            => new PureCosChannel(input);

        [EmmyDoc("Create an absolute value channel")]
        public static AbsChannel Abs(ValueChannel input)
            => new AbsChannel(input);

        [EmmyDoc("Shift the timing input of a channel by another channel's output")]
        public static TimeShiftChannel TimeShift(ValueChannel value, ValueChannel shift)
            => new TimeShiftChannel(value, shift);

        [EmmyDoc("Scale the timing input of a channel by another channel's output")]
        public static TimeScaleChannel TimeScale(ValueChannel value, ValueChannel scale)
            => new TimeScaleChannel(value, scale);

        [EmmyDoc("Chain two channels together by sampling the outer channel with the result of the inner")]
        public static ChainChannel Chain(ValueChannel outer, ValueChannel inner)
            => new ChainChannel(outer, inner);

        [EmmyDoc("Create a channel which returns the current timing")]
        public static TimingChannel Timing()
            => new TimingChannel();

        [EmmyDoc("Inverts a boolean channel")]
        public static NotChannel Not(BooleanChannel input)
            => new NotChannel(input);

        [EmmyDoc("Ands two boolean channel")]
        public static AndChannel And(BooleanChannel a, BooleanChannel b)
            => new AndChannel(a, b);

        [EmmyDoc("Ors two boolean channel")]
        public static OrChannel Or(BooleanChannel a, BooleanChannel b)
            => new OrChannel(a, b);

        [EmmyDoc("Checks if two channels are equal")]
        public static BooleanChannel Equal(ValueChannel a, ValueChannel b)
            => new NumericalComparisonChannel(a, b, ComparisonType.Equals);

        [EmmyDoc("Checks if two string channels are equal")]
        public static BooleanChannel Equal(StringChannel a, StringChannel b)
            => new StringComparisonChannel(a, b, ComparisonType.Equals);

        [EmmyDoc("Switches between one channel to another based on a condition")]
        public static ValueChannel IfElse(BooleanChannel condition, ValueChannel onTrue, ValueChannel onFalse)
            => new IfElseChannel(condition, onTrue, onFalse);

        [EmmyDoc("Checks if one channel is greater than another")]
        public static BooleanChannel GreaterThan(ValueChannel a, ValueChannel b)
            => new NumericalComparisonChannel(a, b, ComparisonType.GreaterThan);

        [EmmyDoc("Checks if one channel is greater than or equal to another")]
        public static BooleanChannel GreaterEqual(ValueChannel a, ValueChannel b)
            => new NumericalComparisonChannel(a, b, ComparisonType.GreaterEqual);

        [EmmyDoc("Checks if one channel is greater than another")]
        public static BooleanChannel LessThan(ValueChannel a, ValueChannel b)
            => new NumericalComparisonChannel(a, b, ComparisonType.LessThan);

        [EmmyDoc("Checks if one channel is greater than or equal to another")]
        public static BooleanChannel LessEqual(ValueChannel a, ValueChannel b)
            => new NumericalComparisonChannel(a, b, ComparisonType.LessEqual);
    }
}