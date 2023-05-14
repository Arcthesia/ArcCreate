namespace ArcCreate.Compose.Navigation
{
    public class MacroAction : IAction
    {
        private readonly string macro;

        public MacroAction(string macro)
        {
            this.macro = macro;
        }

        public string Id => macro;

        public string FullPath => macro;

        public string CategoryI18nName => macro;

        public string I18nName => macro;

        public bool ShouldDisplayOnContextMenu => false;

        public void Execute()
        {
            Services.Macros.RunMacro(macro);
        }
    }
}