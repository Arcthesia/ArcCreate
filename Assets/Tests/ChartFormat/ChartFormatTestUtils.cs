using System.Collections.Generic;
using System.Linq;
using ArcCreate.ChartFormat;
using ArcCreate.ChartFormat.Grammar;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Tests.Unit
{
    public static class ChartFormatTestUtils
    {
        public static List<AntlrEvent> ParseEvents(string raw) => ChartReader.ParseEvents(raw).Events.ToList();

        public static void AssertChartReaderError([InstantHandle] TestDelegate code, ChartError.Kind kind)
        {
            try
            {
                code.Invoke();
                Assert.Fail();
            }
            catch (ChartReaderException e)
            {
                Assert.That(e.ErrorKind, Is.EqualTo(kind));
            }
        }

        public static void AssertChartFileErrors(Result<ChartFileErrors> res, ChartError.Kind kind)
        {
            Assert.That(res.IsError, Is.True);
            Assert.That(res.Error, Is.InstanceOf<ChartFileErrors>());

            bool hasErrorKind = false;
            foreach (var e in res.Error.Errors)
            {
                hasErrorKind = hasErrorKind || (e.ErrorKind == kind);
            }

            Assert.That(hasErrorKind, Is.True);
        }
    }
}