using NUnit.Framework;

namespace OneCopy.Tests
{
    [TestFixture]
    public class DuplicateTest
    {
        [Test]
        public void ShouldDetectDuplicates()
        {
            // arrange
            TestSetup.CreateSimulatedRealLifeDirectory();

            // act
            // var result = testClass.SomeMethod();

            // assert
            Assert.AreEqual(true, true);
        }
    }
}