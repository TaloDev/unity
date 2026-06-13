using NUnit.Framework;

namespace TaloGameServices.Test
{
    [SetUpFixture]
    internal class TestSetup
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            TestModeFlag.IsEnabled = true;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TestModeFlag.IsEnabled = false;
        }
    }
}
