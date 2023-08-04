using UnityEngine;

namespace ArcCreate.Utility.Extension
{
    public static class TextFormat
    {
        public static string FormatPlayCount(int count)
        {
            return $"{OrdinalNumber(count)} PLAY";
        }

        public static string FormatRetryCount(int count)
        {
            return $"{OrdinalNumber(count)} TRY";
        }

        public static string OrdinalNumber(int number)
        {
            number = Mathf.Max(number, 1);
            string suffix = "TH";
            if (number < 10 || number >= 20)
            {
                switch (number % 10)
                {
                    case 1:
                        suffix = "ST";
                        break;
                    case 2:
                        suffix = "ND";
                        break;
                    case 3:
                        suffix = "RD";
                        break;
                }
            }

            return $"{number}{suffix}";
        }
    }
}