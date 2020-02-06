// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2BridgeUnitTests
{
    using System;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using K2Bridge;
    using K2Bridge.Controllers;
    using K2Bridge.KustoConnector;
    using K2Bridge.Models;
    using K2Bridge.Models.Response;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using Substitute = NSubstitute.Substitute;

    [TestFixture]
    public class QueryControllerTests
    {
        private const string ValidQueryContent = "{\"index\":\"kibana_sample_data_flights\",\"ignore_unavailable\":true,\"preference\":1572955935509}\n{\"version\":true,\"size\":500,\"sort\":[{\"timestamp\":{\"order\":\"desc\",\"unmapped_type\":\"boolean\"}}],\"_source\":{\"excludes\":[]},\"aggs\":{\"2\":{\"date_histogram\":{\"field\":\"timestamp\",\"interval\":\"1d\",\"time_zone\":\"America/Los_Angeles\",\"min_doc_count\":1}}},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[{\"field\":\"timestamp\",\"format\":\"date_time\"}],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"timestamp\":{\"gte\":1561673881638,\"lte\":1566712210749,\"format\":\"epoch_millis\"}}}],\"filter\":[],\"should\":[],\"must_not\":[]}},\"highlight\":{\"pre_tags\":[\"@kibana-highlighted-field@\"],\"post_tags\":[\"@/kibana-highlighted-field@\"],\"fields\":{\"*\":{}},\"fragment_size\":2147483647},\"timeout\":\"30000ms\"}";

        private static readonly object[] InValidQueryContent = {
            new TestCaseData("{\"index\":\"kibana_sample_data_flights\",\"ignore_unavailable\":true,\"preference\":1572955935509}").SetName("SearchInternal_WhenNoQuery_IsInvalid"),
            new TestCaseData(null).SetName("SearchInternal_WhenNullValue_IsInvalid"),
            new TestCaseData(string.Empty).SetName("SearchInternal_WhenEmptyValue_IsInvalid"),
        };

        private static readonly object[] IntegrationTestCases = {
            new TestCaseData("{\"index\":\"kibana_sample_data_flights\",\"ignore_unavailable\":true,\"preference\":1572955935509}\n{\"version\":true,\"size\":500,\"sort\":[{\"timestamp\":{\"order\":\"desc\",\"unmapped_type\":\"boolean\"}}],\"_source\":{\"excludes\":[]},\"aggs\":{\"2\":{\"date_histogram\":{\"field\":\"timestamp\",\"interval\":\"1d\",\"time_zone\":\"America/Los_Angeles\",\"min_doc_count\":1}}},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[{\"field\":\"timestamp\",\"format\":\"date_time\"}],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"timestamp\":{\"gte\":1561673881638,\"lte\":1566712210749,\"format\":\"epoch_millis\"}}}],\"filter\":[],\"should\":[],\"must_not\":[]}},\"highlight\":{\"pre_tags\":[\"@kibana-highlighted-field@\"],\"post_tags\":[\"@/kibana-highlighted-field@\"],\"fields\":{\"*\":{}},\"fragment_size\":2147483647},\"timeout\":\"30000ms\"}", typeof(OkObjectResult)).SetName("QueryController_WhenQueryIsValid_ReturnsOk"),
            new TestCaseData(string.Empty, typeof(BadRequestObjectResult)).SetName("QueryController_WhenQueryIsValid_ReturnsOk"),
        };

        [Test]
        public async Task SearchInternal_ReturnsAnActionResult_OKfromKustoAsync()
        {
            // Arrange
            var uat = GetController();

            // Act
            var result = await uat.SearchInternalAsync(true, true, ValidQueryContent);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsInstanceOf<ElasticResponse>(((OkObjectResult)result).Value);
        }

        [Test]
        public void QueryController_WhenNoArgs_ThrowsOnInit()
        {
            // Arrange
            var mockTranslator = new Mock<ITranslator>();
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            var mockQueryExecutor = new Mock<IQueryExecutor>();

            Assert.Throws<ArgumentNullException>(() => {
                new QueryController(mockQueryExecutor.Object, null, mockLogger.Object, mockResponseParser.Object);
            });

            Assert.Throws<ArgumentNullException>(() => {
                new QueryController(null, mockTranslator.Object, mockLogger.Object, mockResponseParser.Object);
            });

            Assert.Throws<ArgumentNullException>(() => {
                new QueryController(mockQueryExecutor.Object, mockTranslator.Object, null, mockResponseParser.Object);
            });

            Assert.Throws<ArgumentNullException>(() => {
                new QueryController(mockQueryExecutor.Object, mockTranslator.Object, mockLogger.Object, null);
            });
        }

        [Test]
        public async Task SearchInternal_OnValidInput_TranslatesAndExecutesQuery()
        {
            // Arrange
            (string header, string query) = ControllerExtractMethods.SplitQueryBody(ValidQueryContent);
            var queryData = new QueryData(query, header);
            var ts = new TimeSpan(1);
            var reader = Substitute.For<IDataReader>();
            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(translator => translator.Translate(
                header, query)).Returns(queryData);
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(queryData)).Returns(Task.FromResult((ts, reader)));
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            mockResponseParser.Setup(exec =>
                exec.Parse(
                    reader,
                    queryData,
                    ts));
            var uat = new QueryController(mockQueryExecutor.Object, mockTranslator.Object, mockLogger.Object, mockResponseParser.Object);

            // Act
            await uat.SearchInternalAsync(true, true, ValidQueryContent);

            // Assert
            mockTranslator.Verify(
                translator => translator.Translate(header, query), Times.Once());
            mockQueryExecutor.Verify(
                 executor => executor.ExecuteQueryAsync(queryData), Times.Once());
            mockResponseParser.Verify(
                parsr => parsr.Parse(reader, queryData, ts), Times.Once());
        }

        [TestCaseSource("InValidQueryContent")]
        public void SearchInternal_OnInvaliddRequestData_FailsArgumentException(string input)
        {
            // Arrange
            var uat = GetController();

            // Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await uat.SearchInternalAsync(true, true, input));
        }

        [TestCaseSource("IntegrationTestCases")]
        public async Task SearchInternal_PostRequest_ReturnsExpectedResult(string content, Type resultType)
        {
            Ensure.IsNotNull(resultType, nameof(resultType));

            // Arrange
            var mockTranslator = new Mock<ITranslator>();
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            var mockQueryExecutor = new Mock<IQueryExecutor>();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Controller needs a controller context
            var controllerContext = new ControllerContext() {
                HttpContext = httpContext,
            };
            var controller = new QueryController(
                mockQueryExecutor.Object,
                mockTranslator.Object,
                mockLogger.Object,
                mockResponseParser.Object) { ControllerContext = controllerContext };

            // Act
            var result = await controller.SearchAsync(true, true);

            // Assert
            Assert.IsInstanceOf(resultType, result, $"result {result.ToString()} is not of expected type {resultType.Name}");
        }

        [Test]
        public async Task Search_PostRequestErrorInTranslator_ReturnsError()
        {
            // Arrange
            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(translate => translate.Translate(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("test"));
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            var mockQueryExecutor = new Mock<IQueryExecutor>();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(ValidQueryContent));

            // Controller needs a controller context
            var controllerContext = new ControllerContext() {
                HttpContext = httpContext,
            };
            var controller = new QueryController(
                mockQueryExecutor.Object,
                mockTranslator.Object,
                mockLogger.Object,
                mockResponseParser.Object) { ControllerContext = controllerContext };

            // Act
            var result = await controller.SearchAsync(true, true);

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result, $"result {result.ToString()} is not of expected type BadRequestObjectResult");
        }

        [Test]
        public async Task Search_PostRequestErrorInParser_ReturnsError()
        {
            // Arrange
            var mockTranslator = new Mock<ITranslator>();
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            mockResponseParser.Setup(parser => parser.Parse(It.IsAny<IDataReader>(), It.IsAny<QueryData>(), It.IsAny<TimeSpan>())).Throws(new Exception("test"));
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(ValidQueryContent));

            // Controller needs a controller context
            var controllerContext = new ControllerContext() {
                HttpContext = httpContext,
            };
            var controller = new QueryController(
                mockQueryExecutor.Object,
                mockTranslator.Object,
                mockLogger.Object,
                mockResponseParser.Object) { ControllerContext = controllerContext };

            // Act
            var result = await controller.SearchAsync(true, true);

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result, $"result {result.ToString()} is not of expected type BadRequestObjectResult");
        }

        [Test]
        public async Task Search_PostRequestErrorInExecutor_ReturnsError()
        {
            // Arrange
            var mockTranslator = new Mock<ITranslator>();
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(executor => executor.ExecuteQueryAsync(It.IsAny<QueryData>())).Throws(new Exception("test"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(ValidQueryContent));

            // Controller needs a controller context
            var controllerContext = new ControllerContext() {
                HttpContext = httpContext,
            };
            var controller = new QueryController(
                mockQueryExecutor.Object,
                mockTranslator.Object,
                mockLogger.Object,
                mockResponseParser.Object) { ControllerContext = controllerContext };

            // Act
            var result = await controller.SearchAsync(true, true);

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result, $"result {result.ToString()} is not of expected type BadRequestObjectResult");
        }

        private QueryController GetController()
        {
            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(translator => translator.Translate(
                It.IsNotNull<string>(), It.IsNotNull<string>())).Returns(new QueryData());
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsAny<QueryData>())).Returns(Task.FromResult((new TimeSpan(), Substitute.For<IDataReader>())));
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            mockResponseParser.Setup(exec =>
                exec.Parse(
                    It.IsAny<IDataReader>(),
                    It.IsAny<QueryData>(),
                    It.IsAny<TimeSpan>()))
                .Returns(new ElasticResponse());

            return new QueryController(mockQueryExecutor.Object, mockTranslator.Object, mockLogger.Object, mockResponseParser.Object);
        }
    }
}