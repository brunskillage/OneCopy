using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneCopy2017.DataObjects;
using OneCopy2017.Services.SimilarImage.Services;
using OneCopy2017.TinyIoc;

namespace OneCopy.MsTests
{
    [TestClass]
    public class DuplicateTests
    {
        private static TinyIoCContainer _container = null;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _container = TestSetup.RegisterServices();

            // the config service uses this static class to populate options usually
            // for the test just do a manual config
            CommandArguments.Directories = new[] {TestSetup.RootDir};

            // uncoment the below to create the test file structure - a one off
            TestSetup.CreateSimulatedRealLifeDirectoryIfNotExists();
        }

        [TestMethod]
        public void ShouldDetectAllFiles()
        {
            // arrange
            var fsService = _container.Resolve<FileSystemService>();

            // act
            var blobs = fsService.GetAllFileBlobs(TestSetup.RootDir, null, null);

            // assert
            fsService.Should().NotBe(null);
            blobs.Count().Should().Be(6);
        }

        [TestMethod]
        public void ShouldApplyExclusionFilterToDirectories()
        {
            // arrange
            var fsService = _container.Resolve<FileSystemService>();
            var filter = "FirstDir|ThirdDir";

            // act
            var directories = fsService.GetAllDirectories(TestSetup.RootDir, filter).ToList();

            // assert
            // should leave the root
            directories.Count.Should().Be(1);
            directories.First().Should().Be(TestSetup.RootDir);
        }

        [TestMethod]
        public void ShouldOnlyContainFilteredFiles()
        {
            // arrange
            var fsService = _container.Resolve<FileSystemService>();

            // act
            var files = fsService.GetAllFileBlobs(TestSetup.RootDir, "jpg|bin", null).ToList();

            // assert
            files.Count.Should().Be(4);

            // should find the filtered files
            files.Count(f => Path.GetExtension(f.FullName).Substring(1).ToUpperInvariant() == "JPG").Should().Be(2);
            files.Count(f => Path.GetExtension(f.FullName).Substring(1).ToUpperInvariant() == "BIN").Should().Be(2);

            // should not find txt files as excluded
            files.Count(f => Path.GetExtension(f.FullName).Substring(1).ToUpperInvariant() == "TXT").Should().Be(0);
        }

        [TestMethod]
        public void ShouldDetectDuplicateWithSameFileNameDifferentDirectory()
        {
            // arrange
            var fsService = _container.Resolve<FileSystemService>();
            var dest = TestSetup.Dupe1SourceFullPath.Replace(TestSetup.RootDir, TestSetup.FirstDir);
            fsService.CopyFile(TestSetup.Dupe1SourceFullPath, dest, true);

            // act
            var blobs = fsService.GetAllFileBlobs(TestSetup.RootDir, null, null).ToList();
            var result = fsService.GetDuplicates(blobs);

            // assert
            result.Count.Should().Be(1);
        }
    }
}