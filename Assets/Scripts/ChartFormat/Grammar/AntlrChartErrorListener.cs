using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using ArcCreate.Utility.Parser;

namespace ArcCreate.ChartFormat
{
    public class AntlrChartErrorListener : BaseErrorListener
    {
        private readonly string[] fileContent;

        public List<ChartError> Errors { get; } = new();

        public AntlrChartErrorListener(string[] fileContent)
        {
            this.fileContent = fileContent;
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
            int charPositionInLine,
            string msg, RecognitionException e)
        {
            Errors.Add(ChartError.Parsing(fileContent[line], line, RawEventType.Unknown,
                new ParsingError("Syntax error", 0, fileContent[line].Length, ParsingError.Kind.Antlr)));
        }
    }
}