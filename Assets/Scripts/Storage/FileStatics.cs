using System.IO;
using UnityEngine;

namespace ArcCreate.Storage
{
    public static class FileStatics
    {
        public const string Database = "arccreate.litedb";
        public const string EditorDatabase = "arccreate.editor.litedb";
        public const string FileStorageLegacy = "storage";
        public const string FileStorage = "files";
        public const string Level = "level";
        public const string Pack = "pack";
        public const string DefaultPackage = "default.arcpkg";
        public const string Temp = "__temp";

#if UNITY_EDITOR
        public static readonly string RootPath = Path.Combine(Application.dataPath, ".imported");
#else
        public static readonly string RootPath = Path.Combine(Application.persistentDataPath, "Persistent");
#endif

        public static readonly string EditorDatabasePath = Path.Combine(Application.persistentDataPath, Database);
        public static readonly string DatabasePath = Path.Combine(RootPath, Database);
        public static readonly string FileStoragePathLegacy = Path.Combine(RootPath, FileStorageLegacy);
        public static readonly string FileStoragePath = Path.Combine(RootPath, FileStorage);
        public static readonly string DefaultPackagePath = Path.Combine(Application.streamingAssetsPath, DefaultPackage);

#if UNITY_EDITOR
        public static readonly string TempPath = Path.Combine(Application.dataPath, ".temporary");
#else
        public static readonly string TempPath = Path.Combine(Application.persistentDataPath, "Temporary");
#endif
        public static readonly string TempImportPath = Path.Combine(TempPath, "import");

#if UNITY_EDITOR
        public static readonly string CheckImportPath = Path.Combine(Application.dataPath, ".checkimport");
#else
        public static readonly string CheckImportPath = Path.Combine(Application.persistentDataPath, "Import");
#endif
    }
}