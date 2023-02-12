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

        public static float Float(string str)
        {
            return (float)Engine.Calculate(str);
        }

        public static int Int(string str)
        {
            return (int)System.Math.Round(Engine.Calculate(str));
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
    }
}