using System;
using System.Collections.Generic;
using System.Globalization;
using Jace;
using Jace.Execution;

namespace ArcCreate.Utility.Parser
{
    /// <summary>
    /// Class for conversion from string to numbers, capable of evaluating math expressions.
    /// </summary>
    public class Evaluator
    {
        private static readonly CalculationEngine Engine = new(CultureInfo.CurrentCulture, ExecutionMode.Interpreted);

        public static bool TryCalculate(string str, IDictionary<string, double> variables, out float value,
            CalculationEngine engine = null)
        {
            engine ??= Engine!;
            try
            {
                value = (float)engine.Calculate(str, variables);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        public static float Calculate(string str, IDictionary<string, double> variables,
            CalculationEngine engine = null)
        {
            engine ??= Engine!;
            return (float)engine.Calculate(str, variables);
        }

        public static bool TryDouble(string str, out double value, CalculationEngine engine = null)
        {
            try
            {
                value = Double(str, engine);
                return true;
            }
            catch
            {
                return double.TryParse(str, out value);
            }
        }

        public static bool TryFloat(string str, out float value, CalculationEngine engine = null)
        {
            try
            {
                value = Float(str, engine);
                return true;
            }
            catch
            {
                return float.TryParse(str, out value);
            }
        }

        public static bool TryInt(string str, out int value, CalculationEngine engine = null)
        {
            try
            {
                value = Int(str, engine);
                return true;
            }
            catch
            {
                return int.TryParse(str, out value);
            }
        }

        private static float Float(string str, CalculationEngine engine = null)
        {
            engine ??= Engine!;
            return (float)engine.Calculate(str);
        }

        private static double Double(string str, CalculationEngine engine = null)
        {
            engine ??= Engine!;
            return engine.Calculate(str);
        }

        private static int Int(string str, CalculationEngine engine = null)
        {
            engine ??= Engine!;
            return (int)Math.Round(engine.Calculate(str));
        }
    }
}