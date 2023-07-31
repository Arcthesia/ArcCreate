using UnityEngine;

namespace ArcCreate.Data
{
    public class Constants
    {
        public const float DropRateScalar = 30;
        public const float MinDropRate = DropRateScalar * 1;
        public const float MaxDropRate = DropRateScalar * 10;
        public const int MinPreviewSegmentLengthMs = 5000;
        public const float GoodPenaltyMultipler = 0.5f;
        public const int MaxScore = 10_000_000;

        private static int androidVersionCode = -1;
        private static bool androidVersionCodeSet = false;

        public static int AndroidVersionCode
        {
            get
            {
                if (!androidVersionCodeSet)
                {
                    using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        androidVersionCode = version.GetStatic<int>("SDK_INT");
                    }

                    androidVersionCodeSet = true;
                }

                return androidVersionCode;
            }
        }
    }
}