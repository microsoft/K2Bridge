using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace K2Bridge.Tests.Integration
{
    [TestClass]
    public class GeneralApiTest
    {
        private readonly HttpClient _client;
        private readonly TestServer _server;

        public GeneralApiTest()
        {
            _server = IntegrationTestHelper.GetIntegrationTestServer("Development");
            _client = _server.CreateClient();
        }

        [TestMethod]
        public async Task SwaggerEndpointIsAvailabe_Successful()
        {
            //arrange
            string requestPath = "swagger/index.html";

            //act
            var response = await _client.GetAsync(requestPath);

            //assert
            response.EnsureSuccessStatusCode();
        }
    }
}
