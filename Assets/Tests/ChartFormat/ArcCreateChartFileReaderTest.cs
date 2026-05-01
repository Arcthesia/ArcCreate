using System.Collections.Generic;
using System.Linq;
using ArcCreate.ChartFormat;
using NSubstitute;
using NUnit.Framework;
using static Tests.Unit.ChartFormatTestUtils;

namespace Tests.Unit
{
    public class ArcCreateChartFileReaderTest
    {
        private ArcCreateChartReader reader;
        private IFileAccessWrapper fileAccess;

        [SetUp]
        public void SetUp()
        {
            fileAccess = Substitute.For<IFileAccessWrapper>();
            reader = (ArcCreateChartReader)ChartReaderFactory.GetReader(fileAccess, "2.acf");
        }

        [Test]
        public void ReadAcfFileWithOneAcfInclude()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "include(incl.acf);");
            SetupFakeFile(
                "incl.acf",
                "(1000,1);");

            reader.Parse();
            List<RawTap> taps = GetRawTapList(reader);

            Assert.That(taps, Has.Count.EqualTo(2));
            Assert.That(taps[0].Timing, Is.Zero);
            Assert.That(taps[1].Timing, Is.EqualTo(1000));
            Assert.That(taps[0].TimingGroup, Is.Zero);
            Assert.That(taps[1].TimingGroup, Is.EqualTo(1));
            Assert.That(reader.TimingGroups, Has.Count.EqualTo(2));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.acf"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("incl.acf"));
            Assert.That(reader.TimingGroups[1].Editable, Is.True);
        }

        [Test]
        public void ReadAcfFileWithTimingGroupWithinAcfInclude()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "include(incl.acf);");
            SetupFakeFile(
                "incl.acf",
                "(1000,1);\n" +
                "timinggroup(){\n" +
                "  timing(0,100.00,4.00);\n" +
                "  (2000,1);\n" +
                "};");

            reader.Parse();
            List<RawTap> taps = GetRawTapList(reader);

            Assert.That(taps, Has.Count.EqualTo(3));
            Assert.That(taps[0].Timing, Is.Zero);
            Assert.That(taps[1].Timing, Is.EqualTo(1000));
            Assert.That(taps[2].Timing, Is.EqualTo(2000));
            Assert.That(taps[0].TimingGroup, Is.Zero);
            Assert.That(taps[1].TimingGroup, Is.EqualTo(1));
            Assert.That(taps[2].TimingGroup, Is.EqualTo(2));
            Assert.That(reader.TimingGroups, Has.Count.EqualTo(3));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.acf"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("incl.acf"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("incl.acf"));
        }

        [Test]
        public void ReadAcfFileWithMultipleAcfInclude()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "include(incl.acf);\n" +
                "include(incl2.acf);");
            SetupFakeFile(
                "incl.acf",
                "(1000,1);");
            SetupFakeFile(
                "incl2.acf",
                "(2000,1);");

            reader.Parse();
            List<RawTap> taps = GetRawTapList(reader);

            Assert.That(taps, Has.Count.EqualTo(3));
            Assert.That(taps[0].Timing, Is.Zero);
            Assert.That(taps[1].Timing, Is.EqualTo(1000));
            Assert.That(taps[2].Timing, Is.EqualTo(2000));
            Assert.That(taps[0].TimingGroup, Is.Zero);
            Assert.That(taps[1].TimingGroup, Is.EqualTo(1));
            Assert.That(taps[2].TimingGroup, Is.EqualTo(2));
            Assert.That(reader.TimingGroups, Has.Count.EqualTo(3));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.acf"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("incl.acf"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("incl2.acf"));
        }

        [Test]
        public void ReadAcfFileWithOneAcfFragment()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "fragment(0,frag.acf);");
            SetupFakeFile(
                "frag.acf",
                "(1000,1);");

            reader.Parse();
            List<RawTap> taps = GetRawTapList(reader);

            Assert.That(taps, Has.Count.EqualTo(2));
            Assert.That(taps[0].Timing, Is.Zero);
            Assert.That(taps[1].Timing, Is.EqualTo(1000));
            Assert.That(taps[0].TimingGroup, Is.Zero);
            Assert.That(taps[1].TimingGroup, Is.EqualTo(1));
            Assert.That(reader.TimingGroups, Has.Count.EqualTo(2));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.acf"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("frag.acf"));
            Assert.That(reader.TimingGroups[1].Editable, Is.False);
        }

        [Test]
        public void ReadAcfFileWithMultipleAcfFragment()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "fragment(0, frag.acf);\n" +
                "fragment(1000, frag2.acf);");
            SetupFakeFile(
                "frag.acf",
                "(1000,1);");
            SetupFakeFile(
                "frag2.acf",
                "(2000,1);");

            reader.Parse();
            List<RawTap> taps = GetRawTapList(reader);

            Assert.That(taps, Has.Count.EqualTo(3));
            Assert.That(taps[0].Timing, Is.Zero);
            Assert.That(taps[1].Timing, Is.EqualTo(1000));
            Assert.That(taps[2].Timing, Is.EqualTo(3000));
            Assert.That(taps[0].TimingGroup, Is.Zero);
            Assert.That(taps[1].TimingGroup, Is.EqualTo(1));
            Assert.That(taps[2].TimingGroup, Is.EqualTo(2));
            Assert.That(reader.TimingGroups, Has.Count.EqualTo(3));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.acf"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("frag.acf"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("frag2.acf"));
            Assert.That(reader.TimingGroups[1].Editable, Is.False);
            Assert.That(reader.TimingGroups[2].Editable, Is.False);
        }

        [Test]
        public void ReadAcfFileWithMultipleAcfFragmentOfSameFile()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "fragment(0, frag.acf);\n" +
                "fragment(1000, frag.acf);");
            SetupFakeFile(
                "frag.acf",
                "(1000,1);");

            reader.Parse();
            List<RawTap> taps = GetRawTapList(reader);

            Assert.That(taps, Has.Count.EqualTo(3));
            Assert.That(taps[0].Timing, Is.Zero);
            Assert.That(taps[1].Timing, Is.EqualTo(1000));
            Assert.That(taps[2].Timing, Is.EqualTo(2000));
            Assert.That(taps[0].TimingGroup, Is.Zero);
            Assert.That(taps[1].TimingGroup, Is.EqualTo(1));
            Assert.That(taps[2].TimingGroup, Is.EqualTo(2));
            Assert.That(reader.TimingGroups, Has.Count.EqualTo(3));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.acf"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("frag.acf"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("frag.acf"));
            Assert.That(reader.TimingGroups[1].Editable, Is.False);
            Assert.That(reader.TimingGroups[2].Editable, Is.False);
        }

        [Test]
        public void ReadAcfFileWithNestedInclude()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "include(dir/incl1.acf);\n");
            SetupFakeFile(
                "dir/incl1.acf",
                "(1000,1);\n" +
                "include(incl2.acf);\n");
            SetupFakeFile(
                "dir/incl2.acf",
                "(2000,1);");

            reader.Parse();

            Assert.That(reader.TimingGroups, Has.Count.EqualTo(3));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.acf"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("dir/incl1.acf").Or.EqualTo("dir\\incl1.acf"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("dir/incl2.acf").Or.EqualTo("dir\\incl2.acf"));
            Assert.That(reader.TimingGroups[1].Editable, Is.True);
            Assert.That(reader.TimingGroups[2].Editable, Is.True);
        }

        [Test]
        public void ReadAcfFileWithNestedIncludeOfSameFileName()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "include(incl1.acf);\n" +
                "include(dir/incl2.acf);\n");
            SetupFakeFile(
                "incl1.acf",
                "(1000,1);");
            SetupFakeFile(
                "dir/incl2.acf",
                "(2000,1);\n" +
                "include(incl1.acf);\n");
            SetupFakeFile(
                "dir/incl1.acf",
                "(3000,1);");

            reader.Parse();

            Assert.That(reader.TimingGroups, Has.Count.EqualTo(4));
        }

        [Test]
        public void ReadAcfFileWithNestedFragment()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "fragment(0,dir/frag1.acf);\n");
            SetupFakeFile(
                "dir/frag1.acf",
                "(1000,1);\n" +
                "fragment(0,frag2.acf);\n");
            SetupFakeFile(
                "dir/frag2.acf",
                "(2000,1);");

            reader.Parse();

            Assert.That(reader.TimingGroups, Has.Count.EqualTo(3));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.acf"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("dir/frag1.acf").Or.EqualTo("dir\\frag1.acf"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("dir/frag2.acf").Or.EqualTo("dir\\frag2.acf"));
            Assert.That(reader.TimingGroups[1].Editable, Is.False);
            Assert.That(reader.TimingGroups[2].Editable, Is.False);
        }

        [Test]
        public void ReadAcfFileFail_CircularDependency()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "include(incl.acf);\n");
            SetupFakeFile(
                "incl.acf",
                "(1000,1);\n" +
                "include(2.acf);");

            AssertChartFileErrors(reader.Parse(), ChartError.Kind.ReferencedFileError);
        }

        [Test]
        public void ReadAcfFileFail_IncludeOneFileMultipleTimes()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "include(incl.acf);\n" +
                "include(incl.acf);");
            SetupFakeFile(
                "incl.acf",
                "(1000,1);");

            AssertChartFileErrors(reader.Parse(), ChartError.Kind.IncludeReferencedMultipleTimes);
        }

        [Test]
        public void ReadAcfFileFail_IncludeAlreadyReferencedFragment()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "fragment(0, frag.acf);\n" +
                "include(frag.acf);");
            SetupFakeFile(
                "frag.acf",
                "(1000,1);");

            AssertChartFileErrors(reader.Parse(), ChartError.Kind.IncludeAReferencedFragment);
        }

        [Test]
        public void ReadAcfFileFail_FragmentOfAlreadyReferencedInclude()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "include(incl.acf);\n" +
                "fragment(0, incl.acf);");
            SetupFakeFile(
                "incl.acf",
                "(1000,1);");

            AssertChartFileErrors(reader.Parse(), ChartError.Kind.IncludeReferencedMultipleTimes);
        }

        [Test]
        public void ReadAcfFileFail_MultipleIncludesInNested()
        {
            SetupFakeFile(
                "2.acf",
                "(0,1);\n" +
                "include(incl.acf);\n" +
                "include(nested.acf);");
            SetupFakeFile(
                "incl.acf",
                "(1000,1);");
            SetupFakeFile(
                "nested.acf",
                "(1000,1);\n" +
                "include(incl.acf);");

            AssertChartFileErrors(reader.Parse(), ChartError.Kind.ReferencedFileError);
        }

        private void SetupFakeFile(string path, string content)
        {
            content =
                "AudioOffset:0\n" +
                "-\n" +
                "timing(0,100.00,4.00);\n" +
                content;
            fileAccess.ReadFileByLines(path).Returns(content.Split('\n'));
            fileAccess.ReadFileByLines(path.Replace("/", "\\")).Returns(content.Split('\n'));
        }

        private static List<RawTap> GetRawTapList(ChartReader reader)
        {
            return reader.Events.Where(e => e is RawTap).Cast<RawTap>().ToList();
        }
    }
}