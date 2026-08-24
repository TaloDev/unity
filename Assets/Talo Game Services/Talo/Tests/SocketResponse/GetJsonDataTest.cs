using NUnit.Framework;

namespace TaloGameServices.Test
{
    internal class GetJsonDataTest
    {
        [Test]
        public void GetJsonData_ReturnsDataValueAsJsonString()
        {
            var response = new SocketResponse("{\"res\":\"v1.channels.message\",\"data\":{\"channel\":{\"id\":1,\"name\":\"test\"},\"message\":\"hello\"}}");

            var data = response.GetJsonData();

            Assert.AreEqual("{\"channel\":{\"id\":1,\"name\":\"test\"},\"message\":\"hello\"}", data);
        }
    }
}
