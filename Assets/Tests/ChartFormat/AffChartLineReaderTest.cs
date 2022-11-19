using System.Linq;
using Arc.ChartFormat;
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
            RawTiming e = reader.ParseTiming($"timing({timing},{bpm},{bpl});");

            Assert.That(e.Timing, Is.EqualTo(timing));
            Assert.That(e.Bpm, Is.EqualTo(bpm));
            Assert.That(e.BeatsPerLine, Is.EqualTo(bpl));
        }

        [TestCase(0, 0, 4)]
        [TestCase(0, 0, 0)]
        [TestCase(0, 100, -1)]
        public void ParseTimingFail(int timing, int bpm, int bpl)
        {
            Assert.That(
                () =>
                {
                    RawTiming e = reader.ParseTiming($"timing({timing},{bpm},{bpl});");
                },
                Throws.Exception.TypeOf<ChartFormatException>());
        }

        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(1000, 4)]
        public void ParseTap(int timing, int lane)
        {
            RawTap e = reader.ParseTap($"({timing},{lane});");

            Assert.That(e.Timing, Is.EqualTo(timing));
            Assert.That(e.Lane, Is.EqualTo(lane));
        }

        [TestCase(0, 1000, 1)]
        [TestCase(0, 1000, 0)]
        public void ParseHold(int timing, int endTiming, int lane)
        {
            RawHold e = reader.ParseHold($"hold({timing},{endTiming},{lane});");

            Assert.That(e.Timing, Is.EqualTo(timing));
            Assert.That(e.EndTiming, Is.EqualTo(endTiming));
            Assert.That(e.Lane, Is.EqualTo(lane));
        }

        [TestCase(0, 0, 4)]
        [TestCase(1000, 0, 4)]
        public void ParseHoldFail(int timing, int endTiming, int lane)
        {
            Assert.That(
                () =>
                {
                    RawHold e = reader.ParseHold($"hold({timing},{endTiming},{lane});");
                }, Throws.Exception.TypeOf<ChartFormatException>());
        }

        [TestCase(0, 1000, 0, 0, 1, 1, "b", 0, true, "sfx.wav")]
        [TestCase(0, 1000, 0, 0, 1, 1, "b", 0, true, "sfx_wav")]
        [TestCase(0, 1000, 0, 0, 1, 1, "b", 0, true, "none")]
        [TestCase(1000, 1000, 0, 0, 1, 1, "b", 0, true, "none")]
        [TestCase(1000, 1000, 0, 0, 1, 1, "b", 1, true, "none")]
        public void ParseArc(int timing, int endTiming, float xS, float xE, float yS, float yE, string type, int color, bool isVoid, string sfx)
        {
            RawArc e = reader.ParseArc(
                $"arc({timing},{endTiming},{xS},{xE},{type},{yS},{yE},{color},{sfx},{(isVoid ? "true" : "false")});");

            Assert.That(e.Timing, Is.EqualTo(timing));
            Assert.That(e.EndTiming, Is.EqualTo(endTiming));
            Assert.That(e.XStart, Is.EqualTo(xS));
            Assert.That(e.XEnd, Is.EqualTo(xE));
            Assert.That(e.YStart, Is.EqualTo(yS));
            Assert.That(e.YEnd, Is.EqualTo(yE));
            Assert.That(e.LineType, Is.EqualTo(type));
            Assert.That(e.Color, Is.EqualTo(color));
            Assert.That(e.IsTrace, Is.EqualTo(isVoid));
            Assert.That(e.Sfx, Is.EqualTo(sfx));
        }

        [TestCase(1000, 0, 0, 0, 1, 1, "b", 0, true, "none")]
        public void ParseArcFail(int timing, int endTiming, float xS, float xE, float yS, float yE, string type, int color, bool isVoid, string sfx)
        {
            Assert.That(
                () =>
                {
                    RawArc e = reader.ParseArc(
                        $"arc({timing},{endTiming},{xS},{xE},{type},{yS},{yE},{color},{sfx},{(isVoid ? "true" : "false")});");
                }, Throws.Exception.TypeOf<ChartFormatException>());
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
                $"arc({start},{end},0,0,b,0,0,0,none,true)[{arctapString}];");

            Assert.That(e.ArcTaps, Has.Count.EqualTo(timings.Length));
            for (int i = 0; i < timings.Length; i++)
            {
                Assert.That(e.ArcTaps[i], Is.EqualTo(timings[i]));
            }
        }

        [TestCase(0, 1000, new int[] { -1 })]
        [TestCase(0, 1000, new int[] { 1001 })]
        public void ParseArcTapFail(int start, int end, int[] timings)
        {
            Assert.That(
                () =>
                {
                    string arctapString = "";
                    foreach (int t in timings)
                        {
                            arctapString += $"arctap({t}),";
                        }

                    arctapString = arctapString.Substring(0, arctapString.Length - 1);

                    RawArc e = reader.ParseArc(
                        $"arc({start},{end},0,0,b,0,0,0,none,true)[{arctapString}];");
                }, Throws.Exception.TypeOf<ChartFormatException>());
        }

        [TestCase(0, 0, 0, 0, 0, 0, 0, "l", 1)]
        [TestCase(1000, 0, 0, 0, 0, 0, 0, "l", 1)]
        [TestCase(0, 1000, 1000, 1000, 0, 0, 0, "l", 1)]
        [TestCase(0, 1000, 1000, 1000, 90, 90, 90, "l", 1)]
        [TestCase(0, 1000, 1000, 1000, 90, 90, 90, "l", 0)]
        public void ParseCamera(int start, float x, float y, float z, float rx, float ry, float rz, string type, int duration)
        {
            RawCamera e = reader.ParseCamera(
                $"camera({start},{x},{y},{z},{rx},{ry},{rz},{type},{duration});");

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
        public void ParseCameraFail(int start, float x, float y, float z, float rx, float ry, float rz, string type, int duration)
        {
            Assert.That(
                () =>
                {
                    RawCamera e = reader.ParseCamera(
                        $"camera({start},{x},{y},{z},{rx},{ry},{rz},{type},{duration});");
                }, Throws.Exception.TypeOf<ChartFormatException>());
        }

        [TestCase(new object[] { 0, 100 })]
        [TestCase(new object[] { "s" })]
        [TestCase(new object[] { 0, "s" })]
        [TestCase(new object[] { 0, "s, d" })]
        public void ParseSceneControl(object[] args)
        {
            string argString = string.Join(",", args.Select(o => (o is string) ? $"\"{o}\"" : o.ToString()));
            RawSceneControl e = reader.ParseSceneControl(
                $"scenecontrol(0,test,{argString});");

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
                $"scenecontrol(0,test);");

            Assert.That(e.Timing, Is.EqualTo(0));
            Assert.That(e.SceneControlTypeName, Is.EqualTo("test"));
            Assert.That(e.Arguments.Count, Is.EqualTo(0));
        }

        [Test]
        public void ParseTimingGroupNoProperty()
        {
            RawTimingGroup e = reader.ParseTimingGroup(
                "timinggroup(){");

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
                "timinggroup(noinput){");

            Assert.That(e.NoInput, Is.True);
        }

        [Test]
        public void ParseTimingGroupNumericProperty()
        {
            RawTimingGroup e = reader.ParseTimingGroup(
                "timinggroup(angleX=30){");

            Assert.That(e.AngleX, Is.EqualTo(30));
        }

        [Test]
        public void ParseTimingGroupMultipleProperty()
        {
            RawTimingGroup e = reader.ParseTimingGroup(
                "timinggroup(angleY=30,noclip){");

            Assert.That(e.NoClip, Is.True);
            Assert.That(e.AngleY, Is.EqualTo(30));
        }

        [TestCase("test.aff")]
        [TestCase("with space.aff")]
        [TestCase("dir/file.aff")]
        public void ParseInclude(string path)
        {
            string f = reader.ParseInclude(
                $"include({path});");

            Assert.That(f, Is.EqualTo(path));
        }

        [TestCase("test.aff")]
        [TestCase("with space.aff")]
        [TestCase("dir/file.aff")]
        public void ParseFragment(string path)
        {
            (int t, string f) = reader.ParseFragment(
                $"fragment(1000, {path});");

            Assert.That(t, Is.EqualTo(1000));
            Assert.That(f, Is.EqualTo(path));
        }
    }
}
