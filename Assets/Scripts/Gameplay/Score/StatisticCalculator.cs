namespace ArcCreate.Gameplay.Score
{
    // See: https://en.m.wikipedia.org/wiki/Algorithms_for_calculating_variance#On-line_algorithm
    public class StatisticCalculator
    {
        private double m2;
        private int count;

        public double Mean { get; private set; }

        public double StandardDeviation => count > 1 ? m2 / (count - 1) : 0;

        public void UpdateStatistics(double newValue)
        {
            count++;
            double delta = newValue - Mean;
            Mean += delta / count;
            double delta2 = newValue - Mean;
            m2 += delta * delta2;
        }

        public void Reset()
        {
            Mean = 0;
            m2 = 0;
            count = 0;
        }
    }
}