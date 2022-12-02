using System.Collections.Generic;
using System.IO;
using ArcCreate.ChartFormat;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Unit
{
    public class ChartSerializerTest
    {
        private ChartSerializer serializer;
        private IFileAccessWrapper fileAccess;

        [SetUp]
        public void SetUp()
        {
            fileAccess = Substitute.For<IFileAccessWrapper>();
            serializer = new ChartSerializer(fileAccess, "");
        }

        [Test]
        public void WriteSingleAffFileTest()
        {
            Stream mainChart = GetStream("2.aff");
            var chart = new List<(RawTimingGroup prop, IEnumerable<RawEvent> events)>
            {
                (new RawTimingGroup() { File = "2.aff" },
                new List<RawEvent>() { Timing(0, 100, 4, 0), Tap(0, 1, 0) }),
            };

            serializer.Write(0, 1, chart);
            string result = GetStreamContent(mainChart);

            Assert.That(result, Does.Contain("timing(0,100.00,4.00);"));
            Assert.That(result, Does.Contain("(0,1);"));
        }

        [Test]
        public void WriteOneIncludeTest()
        {
            Stream mainChart = GetStream("2.aff");
            Stream inclChart = GetStream("incl.aff");
            var chart = new List<(RawTimingGroup prop, IEnumerable<RawEvent> events)>
            {
                (new RawTimingGroup() { File = "2.aff" },
                new List<RawEvent>() { Timing(0, 100, 4, 0), Tap(0, 1, 0), Include("incl.aff", 0) }),
                (new RawTimingGroup() { File = "incl.aff" },
                new List<RawEvent>() { Timing(0, 100, 4, 0), Tap(1000, 1, 0) }),
            };

            serializer.Write(0, 1, chart);
            string resultMain = GetStreamContent(mainChart);
            string resultIncl = GetStreamContent(inclChart);

            Assert.That(resultMain, Does.Contain("(0,1);"));
            Assert.That(resultMain, Does.Contain("include(incl.aff);"));
            Assert.That(resultIncl, Does.Contain("(1000,1);"));
            Assert.That(resultMain, Does.Not.Contain("(1000,1);"));
        }

        [Test]
        public void WriteMultipleIncludeTest()
        {
            Stream mainChart = GetStream("2.aff");
            Stream incl1Chart = GetStream("incl1.aff");
            Stream incl2Chart = GetStream("incl2.aff");
            var chart = new List<(RawTimingGroup prop, IEnumerable<RawEvent> events)>
            {
                (new RawTimingGroup() { File = "2.aff" },
                new List<RawEvent>()
                {
                    Timing(0, 100, 4, 0),
                    Tap(0, 1, 0),
                    Include("incl1.aff", 0),
                    Include("incl2.aff", 0),
                }),

                (new RawTimingGroup() { File = "incl1.aff" },
                new List<RawEvent>() { Timing(0, 100, 4, 0), Tap(1000, 1, 0) }),

                (new RawTimingGroup() { File = "incl2.aff" },
                new List<RawEvent>() { Timing(0, 100, 4, 0), Tap(2000, 1, 0) }),
            };

            serializer.Write(0, 1, chart);
            string resultMain = GetStreamContent(mainChart);
            string resultIncl1 = GetStreamContent(incl1Chart);
            string resultIncl2 = GetStreamContent(incl2Chart);

            Assert.That(resultMain, Does.Contain("(0,1);"));
            Assert.That(resultMain, Does.Contain("include(incl1.aff);"));
            Assert.That(resultMain, Does.Contain("include(incl2.aff);"));
            Assert.That(resultIncl1, Does.Contain("(1000,1);"));
            Assert.That(resultIncl2, Does.Contain("(2000,1);"));
            Assert.That(resultMain, Does.Not.Contain("(1000,1);"));
            Assert.That(resultMain, Does.Not.Contain("(2000,1);"));
        }

        [Test]
        public void WriteNestedIncludeTest()
        {
            Stream mainChart = GetStream("2.aff");
            Stream incl1Chart = GetStream("dir/incl1.aff");
            Stream incl2Chart = GetStream("dir/incl2.aff");

            var chart = new List<(RawTimingGroup prop, IEnumerable<RawEvent> events)>
            {
                (new RawTimingGroup() { File = "2.aff" },
                new List<RawEvent>()
                {
                    Timing(0, 100, 4, 0),
                    Tap(0, 1, 0),
                    Include("dir/incl1.aff", 0),
                }),

                (new RawTimingGroup() { File = "dir/incl1.aff" },
                new List<RawEvent>()
                {
                    Timing(0, 100, 4, 0),
                    Tap(1000, 1, 0),
                    Include("incl2.aff", 0),
                }),

                (new RawTimingGroup() { File = "dir/incl2.aff" },
                new List<RawEvent>() { Timing(0, 100, 4, 0), Tap(2000, 1, 0) }),
            };

            serializer.Write(0, 1, chart);
            string resultMain = GetStreamContent(mainChart);
            string resultIncl1 = GetStreamContent(incl1Chart);
            string resultIncl2 = GetStreamContent(incl2Chart);

            Assert.That(resultMain, Does.Contain("(0,1);"));
            Assert.That(resultMain, Does.Contain("include(dir/incl1.aff);"));
            Assert.That(resultIncl1, Does.Contain("include(incl2.aff);"));
            Assert.That(resultMain, Does.Not.Contain("include(incl2.aff);"));
            Assert.That(resultIncl1, Does.Contain("(1000,1);"));
            Assert.That(resultIncl2, Does.Contain("(2000,1);"));
            Assert.That(resultMain, Does.Not.Contain("(1000,1);"));
            Assert.That(resultMain, Does.Not.Contain("(2000,1);"));
        }

        [Test]
        public void DoesNotWriteFragmentsTest()
        {
            Stream mainChart = GetStream("2.aff");
            _ = GetStream("frag.aff");

            var chart = new List<(RawTimingGroup prop, IEnumerable<RawEvent> events)>
            {
                (new RawTimingGroup() { File = "2.aff" },
                new List<RawEvent>()
                {
                    Timing(0, 100, 4, 0),
                    Tap(0, 1, 0),
                    Fragment(1000, "frag.aff", 0),
                }),

                (new RawTimingGroup() { File = "frag.aff", Editable = false },
                new List<RawEvent>()
                {
                    Timing(0, 100, 4, 0),
                    Tap(1000, 1, 0),
                }),
            };

            serializer.Write(0, 1, chart);
            string resultMain = GetStreamContent(mainChart);

            Assert.That(resultMain, Does.Contain("(0,1);"));
            Assert.That(resultMain, Does.Not.Contain("(1000,1);"));
            fileAccess.DidNotReceive().WriteFile("frag.aff");
        }

        // Configure fileAccess substitute to return a custom stream, whose content can be read later for assertions
        private Stream GetStream(string path)
        {
            MemoryStream stream = new MemoryStream();
            fileAccess.WriteFile(path).Returns(new PersistentStreamWriter(stream));
            return stream;
        }

        private string GetStreamContent(Stream stream)
        {
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }

        private RawTap Tap(int timing, int lane, int tg)
        {
            return new RawTap()
            {
                TimingGroup = tg,
                Timing = timing,
                Lane = lane,
                Type = RawEventType.Tap,
            };
        }

        private RawTiming Timing(int timing, float bpm, float bpl, int tg)
        {
            return new RawTiming()
            {
                TimingGroup = tg,
                Timing = timing,
                Bpm = bpm,
                Divisor = bpl,
                Type = RawEventType.Timing,
            };
        }

        private RawInclude Include(string path, int tg)
        {
            return new RawInclude()
            {
                TimingGroup = tg,
                Timing = 0,
                Type = RawEventType.Include,
                File = path,
            };
        }

        private RawFragment Fragment(int timing, string path, int tg)
        {
            return new RawFragment()
            {
                TimingGroup = tg,
                Timing = timing,
                Type = RawEventType.Fragment,
                File = path,
            };
        }

        /// <summary>
        /// Disable Close() method so test code can read its content.
        /// MemoryStream are not persistent and I don't want to write test code with FileStream.
        /// </summary>
        public class PersistentStreamWriter : StreamWriter
        {
            public PersistentStreamWriter(Stream stream)
                : base(stream)
            {
            }

            public override void Close()
            {
            }
        }
    }
}
