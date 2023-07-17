using System.Linq;
using ArcCreate.ChartFormat;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Unit
{
    public class AffChartLineReaderTest
    {
        private AffChartReader reader;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            reader = new AffChartReader(Substitute.For<IFileAccessWrapper>(), "", "2.aff", "2.aff");
        }

        [TestCase(0, 100, 4)]
        [TestCase(1000, 100, 4)]
        [TestCase(0, 999999, 2)]
        [TestCase(0, -1, 3)]
        public void ParseTiming(int timing, int bpm, int bpl)
        {
            RawTiming e = reader.ParseTiming($"timing({timing},{bpm},{bpl});", 0).UnwrapOrElse(AssertFail<RawTiming>);

            Assert.That(e.Timing, Is.EqualTo(timing));
            Assert.That(e.Bpm, Is.EqualTo(bpm));
            Assert.That(e.Divisor, Is.EqualTo(bpl));
        }

        [TestCase(0, 100, -1)]
        public void ParseTimingFail_DivisorNegative(int timing, int bpm, int bpl)
        {
            AssertError(reader.ParseTiming($"timing({timing},{bpm},{bpl});", 0), ChartError.Kind.DivisorNegative);
        }

        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(1000, 4)]
        public void ParseTap(int timing, int lane)
        {
            RawTap e = reader.ParseTap($"({timing},{lane});", 0).UnwrapOrElse(AssertFail<RawTap>);

            Assert.That(e.Timing, Is.EqualTo(timing));
            Assert.That(e.Lane, Is.EqualTo(lane));
        }

        [TestCase(0, 1000, 1)]
        [TestCase(0, 1000, 0)]
        public void ParseHold(int timing, int endTiming, int lane)
        {
            RawHold e = reader.ParseHold($"hold({timing},{endTiming},{lane});", 0).UnwrapOrElse(AssertFail<RawHold>);

            Assert.That(e.Timing, Is.EqualTo(timing));
            Assert.That(e.EndTiming, Is.EqualTo(endTiming));
            Assert.That(e.Lane, Is.EqualTo(lane));
        }

        [TestCase(0, 0, 4)]
        public void ParseHoldFail_DurationZero(int timing, int endTiming, int lane)
        {
            AssertError(reader.ParseHold($"hold({timing},{endTiming},{lane});", 0), ChartError.Kind.DurationZero);
        }

        [TestCase(1000, 0, 4)]
        public void ParseHoldFail_DurationNegative(int timing, int endTiming, int lane)
        {
            AssertError(reader.ParseHold($"hold({timing},{endTiming},{lane});", 0), ChartError.Kind.DurationNegative);
        }

        [TestCase(0, 1000, 0, 0, 1, 1, "b", 0, true, "sfx.wav")]
        [TestCase(0, 1000, 0, 0, 1, 1, "b", 0, true, "sfx_wav")]
        [TestCase(0, 1000, 0, 0, 1, 1, "b", 0, true, "none")]
        [TestCase(1000, 1000, 0, 0, 1, 1, "b", 0, true, "none")]
        [TestCase(1000, 1000, 0, 0, 1, 1, "b", 1, true, "none")]
        public void ParseArc(int timing, int endTiming, float xS, float xE, float yS, float yE, string type, int color, bool isTrace, string sfx)
        {
            RawArc e = reader.ParseArc(
                $"arc({timing},{endTiming},{xS},{xE},{type},{yS},{yE},{color},{sfx},{(isTrace ? "true" : "false")});", 0)
                .UnwrapOrElse(AssertFail<RawArc>);

            Assert.That(e.Timing, Is.EqualTo(timing));
            Assert.That(e.EndTiming, Is.EqualTo(endTiming));
            Assert.That(e.XStart, Is.EqualTo(xS));
            Assert.That(e.XEnd, Is.EqualTo(xE));
            Assert.That(e.YStart, Is.EqualTo(yS));
            Assert.That(e.YEnd, Is.EqualTo(yE));
            Assert.That(e.LineType, Is.EqualTo(type));
            Assert.That(e.Color, Is.EqualTo(color));
            Assert.That(e.IsTrace, Is.EqualTo(isTrace));
            Assert.That(e.Sfx, Is.EqualTo(sfx));
        }

        [TestCase(1000, 0, 0, 0, 1, 1, "b", 0, true, "none")]
        public void ParseArcFail_DurationNegative(int timing, int endTiming, float xS, float xE, float yS, float yE, string type, int color, bool isTrace, string sfx)
        {
            AssertError(
                reader.ParseArc($"arc({timing},{endTiming},{xS},{xE},{type},{yS},{yE},{color},{sfx},{(isTrace ? "true" : "false")});", 0),
                ChartError.Kind.DurationNegative);
        }

        [TestCase(0, 1000, new int[] { 0 })]
        [TestCase(0, 1000, new int[] { 1000 })]
        [TestCase(0, 1000, new int[] { 500 })]
        [TestCase(0, 1000, new int[] { 0, 500, 1000 })]
        public void ParseArcTap(int start, int end, int[] timings)
        {
            string arctapString = "";
            foreach (int t in timings)
            {
                arctapString += $"arctap({t}),";
            }

            arctapString = arctapString.Substring(0, arctapString.Length - 1);

            RawArc e = reader.ParseArc(
                $"arc({start},{end},0,0,b,0,0,0,none,true)[{arctapString}];", 0)
                .UnwrapOrElse(AssertFail<RawArc>);

            Assert.That(e.ArcTaps, Has.Count.EqualTo(timings.Length));
            for (int i = 0; i < timings.Length; i++)
            {
                Assert.That(e.ArcTaps[i].Timing, Is.EqualTo(timings[i]));
            }
        }

        [TestCase(0, 1000, new int[] { -1 })]
        [TestCase(0, 1000, new int[] { 1001 })]
        public void ParseArcTapFail_OutOfRange(int start, int end, int[] timings)
        {
            string arctapString = "";
            foreach (int t in timings)
            {
                arctapString += $"arctap({t}),";
            }

            arctapString = arctapString.Substring(0, arctapString.Length - 1);

            AssertError(
                reader.ParseArc($"arc({start},{end},0,0,b,0,0,0,none,true)[{arctapString}];", 0),
                ChartError.Kind.ArcTapOutOfRange);
        }

        [TestCase(0, 0, 0, 0, 0, 0, 0, "l", 1)]
        [TestCase(1000, 0, 0, 0, 0, 0, 0, "l", 1)]
        [TestCase(0, 1000, 1000, 1000, 0, 0, 0, "l", 1)]
        [TestCase(0, 1000, 1000, 1000, 90, 90, 90, "l", 1)]
        [TestCase(0, 1000, 1000, 1000, 90, 90, 90, "l", 0)]
        public void ParseCamera(int start, float x, float y, float z, float rx, float ry, float rz, string type, int duration)
        {
            RawCamera e = reader.ParseCamera(
                $"camera({start},{x},{y},{z},{rx},{ry},{rz},{type},{duration});", 0).UnwrapOrElse(AssertFail<RawCamera>);

            Assert.That(e.Timing, Is.EqualTo(start));
            Assert.That(e.Move.x, Is.EqualTo(x));
            Assert.That(e.Move.y, Is.EqualTo(y));
            Assert.That(e.Move.z, Is.EqualTo(z));
            Assert.That(e.Rotate.x, Is.EqualTo(rx));
            Assert.That(e.Rotate.y, Is.EqualTo(ry));
            Assert.That(e.Rotate.z, Is.EqualTo(rz));
            Assert.That(e.CameraType, Is.EqualTo(type));
            Assert.That(e.Duration, Is.EqualTo(duration));
        }

        [TestCase(0, 0, 0, 0, 0, 0, 0, "l", -1)]
        public void ParseCameraFail_DurationNegative(int start, float x, float y, float z, float rx, float ry, float rz, string type, int duration)
        {
            AssertError(
                reader.ParseCamera($"camera({start},{x},{y},{z},{rx},{ry},{rz},{type},{duration});", 0),
                ChartError.Kind.DurationNegative);
        }

        [TestCase(new object[] { 0, 100 })]
        [TestCase(new object[] { "s" })]
        [TestCase(new object[] { 0, "s" })]
        [TestCase(new object[] { 0, "s, d" })]
        public void ParseSceneControl(object[] args)
        {
            string argString = string.Join(",", args.Select(o => (o is string) ? $"\"{o}\"" : o.ToString()));
            RawSceneControl e = reader.ParseSceneControl(
                $"scenecontrol(0,test,{argString});", 0).UnwrapOrElse(AssertFail<RawSceneControl>);

            Assert.That(e.Timing, Is.EqualTo(0));
            Assert.That(e.SceneControlTypeName, Is.EqualTo("test"));
            Assert.That(e.Arguments.Count, Is.EqualTo(args.Length));
            for (int i = 0; i < args.Length; i++)
            {
                Assert.That(args[i], Is.EqualTo(e.Arguments[i]));
            }
        }

        [Test]
        public void ParseSceneControlNoArgs()
        {
            RawSceneControl e = reader.ParseSceneControl(
                $"scenecontrol(0,test);", 0).UnwrapOrElse(AssertFail<RawSceneControl>);

            Assert.That(e.Timing, Is.EqualTo(0));
            Assert.That(e.SceneControlTypeName, Is.EqualTo("test"));
            Assert.That(e.Arguments.Count, Is.EqualTo(0));
        }

        [Test]
        public void ParseTimingGroupNoProperty()
        {
            RawTimingGroup e = reader.ParseTimingGroup(
                "timinggroup(){", 0).UnwrapOrElse(AssertFail<RawTimingGroup>);

            Assert.That(e.NoInput, Is.False);
            Assert.That(e.NoClip, Is.False);
            Assert.That(e.AngleX, Is.Zero);
            Assert.That(e.AngleY, Is.Zero);
            Assert.That(e.Side, Is.EqualTo(SideOverride.None));
        }

        [Test]
        public void ParseTimingGroupOneProperty()
        {
            RawTimingGroup e = reader.ParseTimingGroup(
                "timinggroup(noinput){", 0).UnwrapOrElse(AssertFail<RawTimingGroup>);

            Assert.That(e.NoInput, Is.True);
        }

        [Test]
        public void ParseTimingGroupNumericProperty()
        {
            RawTimingGroup e = reader.ParseTimingGroup(
                "timinggroup(angleX=30){", 0).UnwrapOrElse(AssertFail<RawTimingGroup>);

            Assert.That(e.AngleX, Is.EqualTo(30));
        }

        [Test]
        public void ParseTimingGroupMultipleProperty()
        {
            RawTimingGroup e = reader.ParseTimingGroup(
                "timinggroup(angleY=30,noclip){", 0).UnwrapOrElse(AssertFail<RawTimingGroup>);

            Assert.That(e.NoClip, Is.True);
            Assert.That(e.AngleY, Is.EqualTo(30));
        }

        [TestCase("test.aff")]
        [TestCase("with space.aff")]
        [TestCase("dir/file.aff")]
        public void ParseInclude(string path)
        {
            string f = reader.ParseInclude(
                $"include({path});", 0).UnwrapOrElse(AssertFail<string>);

            Assert.That(f, Is.EqualTo(path));
        }

        [TestCase("test.aff")]
        [TestCase("with space.aff")]
        [TestCase("dir/file.aff")]
        public void ParseFragment(string path)
        {
            RawFragment frag = reader.ParseFragment(
                $"fragment(1000, {path});", 0).UnwrapOrElse(AssertFail<RawFragment>);

            Assert.That(frag.Timing, Is.EqualTo(1000));
            Assert.That(frag.File, Is.EqualTo(path));
        }

        private T AssertFail<T>(ChartError e)
        {
            Assert.Fail(e.Message);
            return default;
        }

        private void AssertError<T>(Result<T, ChartError> res, ChartError.Kind kind)
        {
            Assert.That(res.IsError, Is.True);
            Assert.That(res.Error, Is.InstanceOf<ChartError>());
            Assert.That(res.Error.ErrorKind, Is.EqualTo(kind));
        }
    }
}
