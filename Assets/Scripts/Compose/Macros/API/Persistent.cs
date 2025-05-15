using System;
using ArcCreate.Storage;
using EmmySharp;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization;
using UltraLiteDB;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmySingleton]
    [EmmyGroup("Macros")]
    public class Persistent
    {
        /** Make sure store type doesn't get messed up (trying to deserialize raw string type) */
        private static int StringType = 254865;

        private static bool initialized = false;
        private static UltraLiteDatabase database;
        private static UltraLiteCollection<MacroPersistenceStorage> storage;

        private class MacroPersistenceStorage
        {
            [BsonId] public int Id { get; set; }
            public string Value { get; set; }
        }

        private static void Initialize()
        {
            if (!initialized)
            {
                database = new UltraLiteDatabase(FileStatics.EditorDatabasePath);
                storage = database.GetCollection<MacroPersistenceStorage>();
                initialized = true;
            }
        }


        [EmmyDoc("Get the string data for a given key. Returns defaultValue if valid data can not be found.")]
        public string GetString(string key, string defaultValue = null)
        {
            if (!initialized) Initialize();
            int id = key.GetHashCode() + StringType;
            MacroPersistenceStorage row = storage.FindOne(Query.EQ("_id", id));
            if (row == null) return defaultValue;
            return row.Value;
        }

        [EmmyDoc("Set the string data for a given key.")]
        public void SetString(string key, string value)
        {
            if (!initialized) Initialize();
            int id = key.GetHashCode() + StringType;
            MacroPersistenceStorage row = storage.FindOne(Query.EQ("_id", id));
            if (row == null)
            {
                row = new MacroPersistenceStorage();
                row.Id = id;
            }

            row.Value = value;
            storage.Upsert(row);
        }

        [EmmyDoc("Set the string data for a given key, the value can be any primitive lua types.\\n" +
                 "e.g,. boolean, number, string and table created with lua.")]
        public void Set(string key, DynValue value)
        {
            if (!initialized) Initialize();
            int id = key.GetHashCode();
            MacroPersistenceStorage row = storage.FindOne(Query.EQ("_id", id));
            if (row == null)
            {
                row = new MacroPersistenceStorage();
                row.Id = id;
            }

            row.Value = value.SerializeValue();
            // when serialize fails, it throws ScriptRuntimeException and can be wrapped with pcall to handle in lua
            storage.Upsert(row);
        }

        [EmmyDoc("Get data for a given key, the value will be deserialized and returned.\\n" +
                 "Error when value can't be deserialized, catch it with pcall or xpcall.")]
        public DynValue Get(Script script, string key, DynValue defaultValue)
        {
            if (!initialized) Initialize();
            int id = key.GetHashCode();
            MacroPersistenceStorage row = storage.FindOne(Query.EQ("_id", id));
            if (row == null)
            {
                if (defaultValue == null) return DynValue.Nil;
                return defaultValue;
            }

            try
            {
                DynValue value = script.DoString("return " + row.Value);
                return value;
            }
            catch (Exception e)
            {
                throw new ScriptRuntimeException(e.ToString());
            }
        }

        [EmmyDeprecated("Persistence data no longer saves to json")]
        public void Save()
        {
        }

        [MoonSharpHidden]
        public void Dispose()
        {
            if (initialized)
            {
                database.Dispose();
                database = null;
                initialized = false;
            }
        }
    }
}