namespace ArcCreate.Gameplay.Data
{
    public static class CameraTypeConvert
    {
        public static string ToCameraString(this CameraType type)
        {
            switch (type)
            {
                case CameraType.L: return "l";
                case CameraType.Reset: return "reset";
                case CameraType.Qi: return "qi";
                case CameraType.Qo: return "qo";
                default: return "reset";
            }
        }

        public static CameraType ToCameraType(this string type)
        {
            switch (type)
            {
                case "l": return CameraType.L;
                case "reset": return CameraType.Reset;
                case "qi": return CameraType.Qi;
                case "qo": return CameraType.Qo;
                case "s": return CameraType.L;
                default: return CameraType.Reset;
            }
        }
    }
}
