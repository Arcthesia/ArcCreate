using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmySingleton]
    [EmmyGroup("Macros")]
    public class Macro
    {
        private readonly MacroLuaEnvironment env;
        private readonly Script script;

        public Macro(MacroLuaEnvironment env, Script script)
        {
            this.env = env;
            this.script = script;
        }

        [EmmyDoc("Create a new macro builder. The id should be unique.")]
        public MacroBuilder New(string id)
        {
            return new MacroBuilder(env, script, id);
        }
    }

    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public class MacroBuilder
    {
        private readonly MacroLuaEnvironment env;
        private readonly Script script;
        private readonly string id;
        private string name;
        private string icon = null;
        private string parent = null;
        private DynValue function = null;

        public MacroBuilder(MacroLuaEnvironment env, Script script, string id)
        {
            this.id = id;
            this.env = env;
            this.script = script;
            name = id;
        }

        [EmmyDoc("Set the macro display name.")]
        public MacroBuilder WithName(string name)
        {
            this.name = name;
            return this;
        }

        [EmmyDoc("Set the display icon display code. Should be a Material icon unicode (example: e1666).")]
        public MacroBuilder WithIcon(string icon)
        {
            this.icon = icon;
            return this;
        }

        [EmmyDoc("Set the parent node. The value should be the id of the parent node.")]
        public MacroBuilder WithParent(string parent)
        {
            this.parent = parent;
            return this;
        }

        [EmmyDoc("Set the macro definition. The value is a function that will be executed every time the macro is run (example: `funcion() ... end`)")]
        public MacroBuilder WithDefinition(DynValue function)
        {
            this.function = function;
            return this;
        }

        [EmmyDoc("Add the macro to the application.")]
        public MacroBuilder Add()
        {
            env.AddNode(parent, id, name, icon, function, script);
            return this;
        }

        [EmmyDoc("Create a new macro builder inheriting the same settings from the current instance")]
        public MacroBuilder New(string id)
        {
            return new MacroBuilder(env, script, id)
            {
                name = name,
                icon = icon,
                parent = parent,
                function = function
            };
        }
    }

    [MoonSharpUserData]
    [EmmySingleton]
    [EmmyGroup("Macros")]
    public class Folder
    {
        private readonly MacroLuaEnvironment env;
        private readonly Script script;

        public Folder(MacroLuaEnvironment env, Script script)
        {
            this.env = env;
            this.script = script;
        }

        public FolderBuilder New(string id)
        {
            return new FolderBuilder(env, script, id);
        }

    }

    [MoonSharpUserData]
    [EmmyGroup("Macros")]
    public class FolderBuilder
    {
        private readonly MacroLuaEnvironment env;
        private readonly Script script;
        private readonly string id;
        private string name;
        private string icon = null;
        private string parent = null;

        public FolderBuilder(MacroLuaEnvironment env, Script script, string id)
        {
            this.id = id;
            this.env = env;
            this.script = script;
            name = id;
        }

        [EmmyDoc("Set the macro display name.")]
        public FolderBuilder WithName(string name)
        {
            this.name = name;
            return this;
        }

        [EmmyDoc("Set the display icon display code. Should be a Material icon unicode (example: e1666).")]
        public FolderBuilder WithIcon(string icon)
        {
            this.icon = icon;
            return this;
        }

        [EmmyDoc("Set the parent node. The value should be the id of the parent node.")]
        public FolderBuilder WithParent(string parent)
        {
            this.parent = parent;
            return this;
        }

        [EmmyDoc("Add the folder to the application.")]
        public FolderBuilder Add()
        {
            env.AddNode(parent, id, name, icon, null, script);
            return this;
        }

        [EmmyDoc("Create a new folder builder inheriting the same settings from the current instance")]
        public FolderBuilder New(string id)
        {
            return new FolderBuilder(env, script, id)
            {
                name = name,
                icon = icon,
                parent = parent,
            };
        }
    }
}