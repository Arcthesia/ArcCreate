using System.Collections.Generic;
using System.Linq;
using ArcCreate.ChartFormat;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Unit
{
    public class ChartFileReaderTest
    {
        private ChartReader reader;
        private IFileAccessWrapper fileAccess;

        [SetUp]
        public void SetUp()
        {
            fileAccess = Substitute.For<IFileAccessWrapper>();
            reader = ChartReaderFactory.GetReader(fileAccess, "2.aff");
        }

        [Test]
        public void ReadAffFileWithOneAffInclude()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "include(incl.aff);");
            SetupFakeFile(
                "incl.aff",
                "(1000,1);");

            reader.Parse();
            List<RawTap> taps = GetRawTapList(reader);

            Assert.That(taps, Has.Count.EqualTo(2));
            Assert.That(taps[0].Timing, Is.Zero);
            Assert.That(taps[1].Timing, Is.EqualTo(1000));
            Assert.That(taps[0].TimingGroup, Is.Zero);
            Assert.That(taps[1].TimingGroup, Is.EqualTo(1));
            Assert.That(reader.TimingGroups, Has.Count.EqualTo(2));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.aff"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("incl.aff"));
            Assert.That(reader.TimingGroups[1].Editable, Is.True);
        }

        [Test]
        public void ReadAffFileWithTimingGroupWithinAffInclude()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "include(incl.aff);");
            SetupFakeFile(
                "incl.aff",
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
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.aff"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("incl.aff"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("incl.aff"));
        }

        [Test]
        public void ReadAffFileWithMultipleAffInclude()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "include(incl.aff);\n" +
                "include(incl2.aff);");
            SetupFakeFile(
                "incl.aff",
                "(1000,1);");
            SetupFakeFile(
                "incl2.aff",
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
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.aff"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("incl.aff"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("incl2.aff"));
        }

        [Test]
        public void ReadAffFileWithOneAffFragment()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "fragment(0,frag.aff);");
            SetupFakeFile(
                "frag.aff",
                "(1000,1);");

            reader.Parse();
            List<RawTap> taps = GetRawTapList(reader);

            Assert.That(taps, Has.Count.EqualTo(2));
            Assert.That(taps[0].Timing, Is.Zero);
            Assert.That(taps[1].Timing, Is.EqualTo(1000));
            Assert.That(taps[0].TimingGroup, Is.Zero);
            Assert.That(taps[1].TimingGroup, Is.EqualTo(1));
            Assert.That(reader.TimingGroups, Has.Count.EqualTo(2));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.aff"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("frag.aff"));
            Assert.That(reader.TimingGroups[1].Editable, Is.False);
        }

        [Test]
        public void ReadAffFileWithMultipleAffFragment()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "fragment(0, frag.aff);\n" +
                "fragment(1000, frag2.aff);");
            SetupFakeFile(
                "frag.aff",
                "(1000,1);");
            SetupFakeFile(
                "frag2.aff",
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
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.aff"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("frag.aff"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("frag2.aff"));
            Assert.That(reader.TimingGroups[1].Editable, Is.False);
            Assert.That(reader.TimingGroups[2].Editable, Is.False);
        }

        [Test]
        public void ReadAffFileWithMultipleAffFragmentOfSameFile()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "fragment(0, frag.aff);\n" +
                "fragment(1000, frag.aff);");
            SetupFakeFile(
                "frag.aff",
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
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.aff"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("frag.aff"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("frag.aff"));
            Assert.That(reader.TimingGroups[1].Editable, Is.False);
            Assert.That(reader.TimingGroups[2].Editable, Is.False);
        }

        [Test]
        public void ReadAffFileWithNestedInclude()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "include(dir/incl1.aff);\n");
            SetupFakeFile(
                "dir/incl1.aff",
                "(1000,1);\n" +
                "include(incl2.aff);\n");
            SetupFakeFile(
                "dir/incl2.aff",
                "(2000,1);");

            reader.Parse();

            Assert.That(reader.TimingGroups, Has.Count.EqualTo(3));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.aff"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("dir/incl1.aff"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("dir/incl2.aff"));
            Assert.That(reader.TimingGroups[1].Editable, Is.True);
            Assert.That(reader.TimingGroups[2].Editable, Is.True);
        }

        [Test]
        public void ReadAffFileWithNestedIncludeOfSameFileName()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "include(incl1.aff);\n" +
                "include(dir/incl2.aff);\n");
            SetupFakeFile(
                "incl1.aff",
                "(1000,1);");
            SetupFakeFile(
                "dir/incl2.aff",
                "(2000,1);\n" +
                "include(incl1.aff);\n");
            SetupFakeFile(
                "dir/incl1.aff",
                "(3000,1);");

            reader.Parse();

            Assert.That(reader.TimingGroups, Has.Count.EqualTo(4));
        }

        [Test]
        public void ReadAffFileWithNestedFragment()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "fragment(0,dir/frag1.aff);\n");
            SetupFakeFile(
                "dir/frag1.aff",
                "(1000,1);\n" +
                "fragment(0,frag2.aff);\n");
            SetupFakeFile(
                "dir/frag2.aff",
                "(2000,1);");

            reader.Parse();

            Assert.That(reader.TimingGroups, Has.Count.EqualTo(3));
            Assert.That(reader.TimingGroups[0].File, Is.EqualTo("2.aff"));
            Assert.That(reader.TimingGroups[1].File, Is.EqualTo("dir/frag1.aff"));
            Assert.That(reader.TimingGroups[2].File, Is.EqualTo("dir/frag2.aff"));
            Assert.That(reader.TimingGroups[1].Editable, Is.False);
            Assert.That(reader.TimingGroups[2].Editable, Is.False);
        }

        [Test]
        public void ReadAffFileFail_CircularDependency()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "include(incl.aff);\n");
            SetupFakeFile(
                "incl.aff",
                "(1000,1);\n" +
                "include(2.aff);");

            Assert.That(
                () =>
                {
                    reader.Parse();
                }, Throws.Exception.TypeOf<ChartFormatException>());
        }

        [Test]
        public void ReadAffFileFail_IncludeOneFileMultipleTimes()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "include(incl.aff);\n" +
                "include(incl.aff);");
            SetupFakeFile(
                "incl.aff",
                "(1000,1);");

            Assert.That(
                () =>
                {
                    reader.Parse();
                }, Throws.Exception.TypeOf<ChartFormatException>());
        }

        [Test]
        public void ReadAffFileFail_IncludeAlreadyReferencedFragment()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "fragment(0, frag.aff);\n" +
                "include(frag.aff);");
            SetupFakeFile(
                "frag.aff",
                "(1000,1);");

            Assert.That(
                () =>
                {
                    reader.Parse();
                }, Throws.Exception.TypeOf<ChartFormatException>());
        }

        [Test]
        public void ReadAffFileFail_FragmentOfAlreadyReferencedInclude()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "include(incl.aff);\n" +
                "fragment(0, incl.aff);");
            SetupFakeFile(
                "incl.aff",
                "(1000,1);");

            Assert.That(
                () =>
                {
                    reader.Parse();
                }, Throws.Exception.TypeOf<ChartFormatException>());
        }

        [Test]
        public void ReadAffFileFail_MultipleIncludesInNested()
        {
            SetupFakeFile(
                "2.aff",
                "(0,1);\n" +
                "include(incl.aff);\n" +
                "include(nested.aff);");
            SetupFakeFile(
                "incl.aff",
                "(1000,1);");
            SetupFakeFile(
                "nested.aff",
                "(1000,1);\n" +
                "include(incl.aff);");

            Assert.That(
                () =>
                {
                    reader.Parse();
                }, Throws.Exception.TypeOf<ChartFormatException>());
        }

        private void SetupFakeFile(string path, string content)
        {
            content =
                "AudioOffset:0\n" +
                "-\n" +
                "timing(0,100.00,4.00);\n" +
                content;
            fileAccess.ReadFileByLines(path).Returns(content.Split('\n'));
        }

        private List<RawTap> GetRawTapList(ChartReader reader)
        {
            return reader.Events.Where(e => e is RawTap).Cast<RawTap>().ToList();
        }
    }
}
