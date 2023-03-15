using System.IO;
using UltraLiteDB;
using UnityEngine;

namespace ArcCreate.Storage
{
    public class Database
    {
        public static UltraLiteDatabase Current { get; private set; }

        public static void Initialize(string path = null)
        {
            BsonMapper.Global.RegisterType<Color>(
                serialize: (color) => ColorUtility.ToHtmlStringRGBA(color),
                deserialize: (value) =>
                {
                    ColorUtility.TryParseHtmlString((string)value, out Color c);
                    return c;
                });

            path = path ?? FileStatics.DatabasePath;
            if (Current == null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                Current = new UltraLiteDatabase(path);
            }
        }

        public static void Dispose()
        {
            Current?.Dispose();
            Current = null;
        }

        public static void Clear()
        {
            foreach (string colNames in Current.GetCollectionNames())
            {
                Current.DropCollection(colNames);
            }
        }
    }
}