using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
