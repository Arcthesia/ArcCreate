using System.Collections;
using System.Collections.Generic;
using Jace;

namespace ArcCreate.Utility.Parser
{
    /// <summary>
    /// Class for conversion from string to numbers, capable of evaluating math expressions.
    /// </summary>
    public class Evaluator
    {
        private static readonly CalculationEngine Engine =
            new CalculationEngine(System.Globalization.CultureInfo.CurrentCulture, Jace.Execution.ExecutionMode.Interpreted);

        public static bool TryCalculate(string str, IDictionary<string, double> variables, out float value)
        {
            try
            {
                value = (float)Engine.Calculate(str, variables);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        public static float Calculate(string str, IDictionary<string, double> variables)
        {
            return (float)Engine.Calculate(str, variables);
        }

        public static bool TryDouble(string str, out double value)
        {
            try
            {
                value = Double(str);
                return true;
            }
            catch
            {
                return double.TryParse(str, out value);
            }
        }

        public static bool TryFloat(string str, out float value)
        {
            try
            {
                value = Float(str);
                return true;
            }
            catch
            {
                return float.TryParse(str, out value);
            }
        }

        public static bool TryInt(string str, out int value)
        {
            try
            {
                value = Int(str);
                return true;
            }
            catch
            {
                return int.TryParse(str, out value);
            }
        }

        private static float Float(string str)
        {
            return (float)Engine.Calculate(str);
        }

        private static double Double(string str)
        {
            return Engine.Calculate(str);
        }

        private static int Int(string str)
        {
            return (int)System.Math.Round(Engine.Calculate(str));
        }
    }
}