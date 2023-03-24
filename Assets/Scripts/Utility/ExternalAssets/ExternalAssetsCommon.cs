using System.IO;
using UnityEngine;

namespace ArcCreate.Utility.ExternalAssets
{
    public static class ExternalAssetsCommon
    {
        public static readonly string SkinFolderPath = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, "Skin");
    }
}