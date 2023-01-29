using System;

namespace ArcCreate.Compose.Navigation
{
    public class EditorScope
    {
        public EditorScope(Type type, string scopeName, object scopeInstance)
        {
            Type = type;
            Id = scopeName;
            Instance = scopeInstance;
            I18nName = $"{Values.NavigationI18nPrefix}.{scopeName}.Name";
        }

        public Type Type { get; private set; }

        public string Id { get; private set; }

        public object Instance { get; private set; }

        public string I18nName { get; private set; }
    }
}