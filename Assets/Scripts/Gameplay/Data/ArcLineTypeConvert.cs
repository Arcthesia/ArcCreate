namespace ArcCreate.Gameplay.Data
{
    public static class ArcLineTypeConvert
    {
        public static string ToLineTypeString(this ArcLineType type)
        {
            switch (type)
            {
                case ArcLineType.B: return "b";
                case ArcLineType.S: return "s";
                case ArcLineType.Si: return "si";
                case ArcLineType.SiSi: return "sisi";
                case ArcLineType.SiSo: return "siso";
                case ArcLineType.So: return "so";
                case ArcLineType.SoSi: return "sosi";
                case ArcLineType.SoSo: return "soso";
                default: return "s";
            }
        }

        public static ArcLineType ToArcLineType(this string type)
        {
            switch (type)
            {
                case "b": return ArcLineType.B;
                case "s": return ArcLineType.S;
                case "si": return ArcLineType.Si;
                case "so": return ArcLineType.So;
                case "sisi": return ArcLineType.SiSi;
                case "siso": return ArcLineType.SiSo;
                case "sosi": return ArcLineType.SoSi;
                case "soso": return ArcLineType.SoSo;
                default: return ArcLineType.S;
            }
        }
    }
}
