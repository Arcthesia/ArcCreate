using UnityEngine;

namespace ArcCreate.Utility
{
    public class RapidAction
    {
        private readonly int decreaseRate;
        private int currentCount;
        private int i;

        public RapidAction(int startingCount, int decreaseRate)
        {
            this.decreaseRate = decreaseRate;
            currentCount = startingCount;
            i = 0;
        }

        public bool ShouldExecute()
        {
            i--;
            if (i <= 0)
            {
                currentCount = Mathf.Max(1, currentCount - decreaseRate);
                i = currentCount;
                return true;
            }

            return false;
        }
    }
}