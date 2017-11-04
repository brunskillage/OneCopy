using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OneCopy.MsTests
{
    [TestClass]
    public class DuplicateTests
    {
        [ClassInitialize]
        public void OnInitialise()
        {
            TestSetup.ClearTestFiles();
            TestSetup.CreateSimulatedRealLifeDirectory();
        }

        [TestMethod]
        public void ShouldDetectAllFiles()
        {
            // arrange
            // var testClass = new TestObject();

            // act
            //var result = testClass.SomeMethod();

            // assert
            // result.Should().Be(true);
        }
    }
}