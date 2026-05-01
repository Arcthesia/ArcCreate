namespace ArcCreate.ChartFormat.Grammar
{
    public interface IAntlrPositionTrack
    {
        int LineNumber { get; }
        int ColumnNumber { get; }
    }
}