// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2BridgeUnitTests
{
    using K2Bridge;
    using K2Bridge.Controllers;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models.Response;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Substitute = NSubstitute.Substitute;

    [TestFixture]
    public class QueryControllerTests
    {
        private const string ValidQueryContent = "{\"index\":\"kibana_sample_data_flights\",\"ignore_unavailable\":true,\"preference\":1572955935509}\n{\"version\":true,\"size\":500,\"sort\":[{\"timestamp\":{\"order\":\"desc\",\"unmapped_type\":\"boolean\"}}],\"_source\":{\"excludes\":[]},\"aggs\":{\"2\":{\"date_histogram\":{\"field\":\"timestamp\",\"interval\":\"1d\",\"time_zone\":\"America/Los_Angeles\",\"min_doc_count\":1}}},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[{\"field\":\"timestamp\",\"format\":\"date_time\"}],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"timestamp\":{\"gte\":1561673881638,\"lte\":1566712210749,\"format\":\"epoch_millis\"}}}],\"filter\":[],\"should\":[],\"must_not\":[]}},\"highlight\":{\"pre_tags\":[\"@kibana-highlighted-field@\"],\"post_tags\":[\"@/kibana-highlighted-field@\"],\"fields\":{\"*\":{}},\"fragment_size\":2147483647},\"timeout\":\"30000ms\"}";
        private const string InValidQueryContent = "{\"index\":\"kibana_sample_data_flights\",\"ignore_unavailable\":true,\"preference\":1572955935509}";

        private QueryController GetController()
        {
            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(translator => translator.Translate(
                It.IsNotNull<string>(), It.IsNotNull<string>())).Returns(new QueryData());
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQuery(It.IsAny<QueryData>())).Returns((new TimeSpan(), Substitute.For<IDataReader>()));
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            mockResponseParser.Setup(exec =>
                exec.ParseElasticResponse(
                    It.IsAny<IDataReader>(),
                    It.IsAny<QueryData>(),
                    It.IsAny<TimeSpan>()))
                .Returns(new ElasticResponse());

            return new QueryController(mockQueryExecutor.Object, mockTranslator.Object, mockLogger.Object, mockResponseParser.Object);
        }

        [Test]
        public async Task Search_ReturnsAnActionResult_OKfromKusto()
        {
            // Arrange
            var queryInBodyPayload = ValidQueryContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Act
            var uat = GetController();
            var result = await uat.SearchInternal(true, true, queryInBodyPayload);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsInstanceOf<ElasticResponse>(((OkObjectResult)result).Value);
        }

        [Test]
        public void Search_ReturnsAnActionResult_FailsInvalidRequestData()
        {
            // Arrange
            var queryInBodyPayload = InValidQueryContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Act
            var uat = GetController();

            // Assert
            Assert.ThrowsAsync<ArgumentException>(() => uat.SearchInternal(true, true, queryInBodyPayload));
        }
    }
}