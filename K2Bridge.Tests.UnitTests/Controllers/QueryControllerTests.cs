// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Controllers
{
    using System;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using global::K2Bridge;
    using global::K2Bridge.Controllers;
    using global::K2Bridge.KustoDAL;
    using global::K2Bridge.Models;
    using global::K2Bridge.Models.Response;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

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
        public async Task SearchInternalAsync_WithOKfromKustoAsync_ReturnsOkActionResult()
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
        public void QueryControllerConstructor_WithNoArgs_ThrowsOnInit()
        {
            // Arrange
            var mockTranslator = new Mock<ITranslator>();
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            var mockQueryExecutor = new Mock<IQueryExecutor>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                new QueryController(mockQueryExecutor.Object, null, mockLogger.Object, mockResponseParser.Object);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                new QueryController(null, mockTranslator.Object, mockLogger.Object, mockResponseParser.Object);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                new QueryController(mockQueryExecutor.Object, mockTranslator.Object, null, mockResponseParser.Object);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                new QueryController(mockQueryExecutor.Object, mockTranslator.Object, mockLogger.Object, null);
            });
        }

        [Test]
        public async Task SearchInternalAsync_WithValidInput_TranslatesAndExecutesQuery()
        {
            // Arrange
            (string header, string query) = ControllerExtractMethods.SplitQueryBody(ValidQueryContent);
            var queryData = new QueryData(query, header);
            var ts = new TimeSpan(1);
            var reader = new Mock<IDataReader>();
            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(translator => translator.Translate(
                header, query)).Returns(queryData);
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(queryData, It.IsAny<RequestContext>())).Returns(Task.FromResult((ts, reader.Object)));
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            mockResponseParser.Setup(exec =>
                exec.Parse(
                    reader.Object,
                    queryData,
                    ts));

            var uat = new QueryController(mockQueryExecutor.Object, mockTranslator.Object, mockLogger.Object, mockResponseParser.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            const string correlationIdHeader = "x-correlation-id";
            uat.Request.Headers[correlationIdHeader] = Guid.NewGuid().ToString();

            // Act
            await uat.SearchInternalAsync(true, true, ValidQueryContent);

            // Assert
            mockTranslator.Verify(
                translator => translator.Translate(header, query), Times.Once());
            mockQueryExecutor.Verify(
                 executor => executor.ExecuteQueryAsync(queryData, It.IsAny<RequestContext>()), Times.Once());
            mockResponseParser.Verify(
                parsr => parsr.Parse(reader.Object, queryData, ts), Times.Once());
        }

        [TestCaseSource(nameof(InValidQueryContent))]
        public void SearchInternalAsync_WithInvaliddRequestData_ThrowsArgumentException(string input)
        {
            // Arrange
            var uat = GetController();

            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await uat.SearchInternalAsync(true, true, input));
        }

        [TestCaseSource(nameof(IntegrationTestCases))]
        public async Task SearchInternalAsync_WithValidRequest_ReturnsExpectedResult(string content, Type resultType)
        {
            Ensure.IsNotNull(resultType, nameof(resultType));

            // Arrange
            var mockTranslator = new Mock<ITranslator>();
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            var mockQueryExecutor = new Mock<IQueryExecutor>();

            var httpContext = new DefaultHttpContext();

            const string correlationIdHeader = "x-correlation-id";
            httpContext.Request.Headers[correlationIdHeader] = Guid.NewGuid().ToString();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Controller needs a controller context
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var controller = new QueryController(
                mockQueryExecutor.Object,
                mockTranslator.Object,
                mockLogger.Object,
                mockResponseParser.Object)
            { ControllerContext = controllerContext };

            // Act
            var result = await controller.SearchAsync(true, true);

            // Assert
            Assert.IsInstanceOf(resultType, result, $"result {result} is not of expected type {resultType.Name}");
        }

        [Test]
        public async Task SearchAsync_WithErrorInTranslator_ReturnsError()
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
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var controller = new QueryController(
                mockQueryExecutor.Object,
                mockTranslator.Object,
                mockLogger.Object,
                mockResponseParser.Object)
            { ControllerContext = controllerContext };

            // Act
            var result = await controller.SearchAsync(true, true);

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result, $"result {result.ToString()} is not of expected type BadRequestObjectResult");
        }

        [Test]
        public async Task SearchAsync_WithErrorInParser_ReturnsError()
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
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            var controller = new QueryController(
                mockQueryExecutor.Object,
                mockTranslator.Object,
                mockLogger.Object,
                mockResponseParser.Object)
            { ControllerContext = controllerContext };

            // Act
            var result = await controller.SearchAsync(true, true);

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result, $"result {result.ToString()} is not of expected type BadRequestObjectResult");
        }

        [Test]
        public async Task SearchAsync_WithErrorInExecutor_ReturnsError()
        {
            // Arrange
            var mockTranslator = new Mock<ITranslator>();
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(executor => executor.ExecuteQueryAsync(It.IsAny<QueryData>(), It.IsAny<RequestContext>())).Throws(new Exception("test"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(ValidQueryContent));

            // Controller needs a controller context
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            var controller = new QueryController(
                mockQueryExecutor.Object,
                mockTranslator.Object,
                mockLogger.Object,
                mockResponseParser.Object)
            { ControllerContext = controllerContext };

            // Act
            var result = await controller.SearchAsync(true, true);

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result, $"result {result} is not of expected type BadRequestObjectResult");
        }

        private QueryController GetController()
        {
            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(translator => translator.Translate(
                It.IsNotNull<string>(), It.IsNotNull<string>())).Returns(default(QueryData));
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(exec => exec.ExecuteQueryAsync(It.IsAny<QueryData>(), It.IsAny<RequestContext>())).Returns(Task.FromResult((default(TimeSpan), new Mock<IDataReader>().Object)));
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            mockResponseParser.Setup(exec =>
                exec.Parse(
                    It.IsAny<IDataReader>(),
                    It.IsAny<QueryData>(),
                    It.IsAny<TimeSpan>()))
                .Returns(new ElasticResponse());

            var ctr = new QueryController(mockQueryExecutor.Object, mockTranslator.Object, mockLogger.Object, mockResponseParser.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            const string correlationIdHeader = "x-correlation-id";
            ctr.Request.Headers[correlationIdHeader] = Guid.NewGuid().ToString();

            return ctr;
        }
    }
}