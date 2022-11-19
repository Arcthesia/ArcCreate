using Jace;

public class Evaluator
{
    private static readonly CalculationEngine Engine = new CalculationEngine();

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
            value = 0;
            return false;
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
            value = 0;
            return false;
        }
    }
}
