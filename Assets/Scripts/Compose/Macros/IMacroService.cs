namespace ArcCreate.Compose.Macros
{
    public interface IMacroService
    {
        void CreateDialog(string dialogTitle, DialogField[] fields, MacroRequest request);

        void RunMacro(string macro);

        void RequestSelection(EventSelectionConstraint constraint, EventSelectionRequest request, bool v);

        void RequestTrackLane(MacroRequest request);

        void RequestTrackPosition(MacroRequest request, int timing);

        void RequestTrackTiming(MacroRequest request);
    }
}