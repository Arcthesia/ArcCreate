using ArcCreate.ChartFormat;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using static Tests.Unit.ChartFormatTestUtils;

namespace Tests.Unit
{
    public class ArcaeaChartFormatTest
    {
        private ArcaeaChartReader reader;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            reader = new ArcaeaChartReader(Substitute.For<IFileAccessWrapper>(), "", "2.aff", "2.aff");
        }

        /// <summary>
        /// See <see cref="ParsingFormula"/> for detailed algorithm implementation
        /// </summary>
        [TestCase("3.000", 3.5)] // chart objects with floated lane are processed differently
        [TestCase("3", 3.0)] // with those in integer lane
        [TestCase("0.500", 1.0)]
        public void ConvertFloatedKeyboard(string arcaeaLane, double expectedLane)
        {
            var events = ParseEvents(
                $"(0,{arcaeaLane});" +
                $"hold(0,100,{arcaeaLane});");

            var evtTap = events[0];
            var evtHold = events[1];

            var tap = reader.ParseTap(evtTap, 0);
            var hold = reader.ParseHold(evtHold, 0);

            Assert.True(Mathf.Approximately(tap.Lane, (float)expectedLane));
            Assert.That(Mathf.Approximately(hold.Lane, (float)expectedLane));
        }

        [TestCase("timinggroup(noinput_anglex1800){};", 180)]
        [TestCase("timinggroup(anglex900){};", 90f)]
        [TestCase("timinggroup(noinput_angley3450){};", 0, -345f)]
        [TestCase("timinggroup(noinput_angley150){};", 0, -15f)]
        [TestCase("timinggroup(noinput_anglex75_angley3525){};", 7.5f, -352.5f)]
        public void ConvertAngleXY(string raw, float expectedAngleX = 0, float expectedAngleY = 0)
        {
            var evt = ParseEvents(raw)[0];
            var (tg, _) = reader.ParseTimingGroup(evt, 0);

            Assert.True(Mathf.Approximately(expectedAngleX, tg.AngleX));
            Assert.True(Mathf.Approximately(expectedAngleY, tg.AngleY));
        }

        [Test]
        public void ParseDesignant()
        {
            var evt = ParseEvents("arc(0,1,0,0,s,1,1,1,none,designant)[arctap(0)];")[0];
            var arc = reader.ParseArc(evt, 0);

            Assert.That(arc.TraceColor, Is.EqualTo(ArcaeaChartReader.DesignantColor));
        }

        [Test]
        public void ParseArcTapWithWidthArgument_Fail()
        {
            var evt = ParseEvents("arc(0,1,0,0,s,1,1,1,none,true)[arctap(0,2)];")[0];

            AssertChartReaderError(() => reader.ParseArc(evt, 0), ChartError.Kind.Parsing);
        }

        [Test]
        public void ParseArcTapWithWidth()
        {
            var evt = ParseEvents("arc(8550,8550,0.00,1.00,s,1.00,1.00,3,none,false,2);")[0];
            var arc = reader.ParseArc(evt, 0);

            Assert.That(arc.ArcTaps[0].Width, Is.EqualTo((float)2));
        }

        [Test]
        public void ParseTimingGroupNoProperty()
        {
            var evt = ParseEvents("timinggroup(){};")[0];

            var (e, _) = reader.ParseTimingGroup(evt, 0);

            Assert.That(e.NoInput, Is.False);
            Assert.That(e.NoClip, Is.False);
            Assert.That(e.AngleX, Is.Zero);
            Assert.That(e.AngleY, Is.Zero);
            Assert.That(e.Side, Is.EqualTo(SideOverride.None));
        }

        [Test]
        public void ParseTimingGroupOneProperty()
        {
            var evt = ParseEvents("timinggroup(noinput){};")[0];

            var (e, _) = reader.ParseTimingGroup(evt, 0);

            Assert.That(e.NoInput, Is.True);
        }

        [Test]
        public void ParseTimingGroupNumericProperty()
        {
            var evt = ParseEvents("timinggroup(anglex300){};")[0];

            var (e, _) = reader.ParseTimingGroup(evt, 0);

            Assert.That(e.AngleX, Is.EqualTo(30));
        }

        [Test]
        public void ParseTimingGroupMultipleProperty()
        {
            var evt = ParseEvents("timinggroup(angley300_noinput){};")[0];

            var (e, _) = reader.ParseTimingGroup(evt, 0);

            Assert.That(e.NoInput, Is.True);
            Assert.That(e.AngleY, Is.EqualTo(-30));
        }

        [TestCase("gimmick-timinggroup")]
        [TestCase("gimmick_timinggroup")]
        public void ParseTimingGroupNameProperty(string id)
        {
            // `gimmick` is the 'identifier' of this tg
            // cause Arcaea doesn't support name="xxx"
            var evt = ParseEvents($"timinggroup({id}_angley300_noinput)" + "{};")[0];

            var (e, _) = reader.ParseTimingGroup(evt, 0);

            Assert.That(e.NoInput, Is.True);
            Assert.That(e.AngleY, Is.EqualTo(-30));

            // `gimmick` is not parsed into tg's name
            // it's just an identifier for the charter
            // 
            // Assert.That(e.Name, Is.EqualTo("gimmick"));
        }
    }
}