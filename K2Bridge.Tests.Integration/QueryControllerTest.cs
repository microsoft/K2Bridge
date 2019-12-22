using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

namespace K2Bridge.Tests.Integration
{
    [TestClass]
    public class QueryControllerTest
    {
        private readonly HttpClient _client;
        private readonly TestServer _server;

        public QueryControllerTest()
        {
            _server = IntegrationTestHelper.GetIntegrationTestServer("Development");
            _client = _server.CreateClient();
        }

        private const string ValidQueryContent = "{\"index\":\"kibana_sample_data_flights\",\"ignore_unavailable\":true,\"preference\":1572955935509}\n{\"version\":true,\"size\":500,\"sort\":[{\"timestamp\":{\"order\":\"desc\",\"unmapped_type\":\"boolean\"}}],\"_source\":{\"excludes\":[]},\"aggs\":{\"2\":{\"date_histogram\":{\"field\":\"timestamp\",\"interval\":\"1d\",\"time_zone\":\"America/Los_Angeles\",\"min_doc_count\":1}}},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[{\"field\":\"timestamp\",\"format\":\"date_time\"}],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"timestamp\":{\"gte\":1561673881638,\"lte\":1566712210749,\"format\":\"epoch_millis\"}}}],\"filter\":[],\"should\":[],\"must_not\":[]}},\"highlight\":{\"pre_tags\":[\"@kibana-highlighted-field@\"],\"post_tags\":[\"@/kibana-highlighted-field@\"],\"fields\":{\"*\":{}},\"fragment_size\":2147483647},\"timeout\":\"30000ms\"}";

        [TestMethod]
        public async Task QuerySearch_Successful()
        {
            //arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "_msearch");
            request.Content = new StringContent(ValidQueryContent, Encoding.UTF8, "application/json");

            //act
            var response = await _client.SendAsync(request);

            //assert
            response.EnsureSuccessStatusCode();
        }
    }
}
