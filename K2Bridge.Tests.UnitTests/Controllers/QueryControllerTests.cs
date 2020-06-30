﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
    using UnitTests.K2Bridge.JsonConverters;

    [TestFixture]
    public class QueryControllerTests
    {
        private const string ValidMSearchRequestContent = "{\"index\":\"kibana_sample_data_flights\",\"ignore_unavailable\":true,\"preference\":1572955935509}\n{\"version\":true,\"size\":500,\"sort\":[{\"timestamp\":{\"order\":\"desc\",\"unmapped_type\":\"boolean\"}}],\"_source\":{\"excludes\":[]},\"aggs\":{\"2\":{\"date_histogram\":{\"field\":\"timestamp\",\"interval\":\"1d\",\"time_zone\":\"America/Los_Angeles\",\"min_doc_count\":1}}},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[{\"field\":\"timestamp\",\"format\":\"date_time\"}],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"timestamp\":{\"gte\":1561673881638,\"lte\":1566712210749,\"format\":\"epoch_millis\"}}}],\"filter\":[],\"should\":[],\"must_not\":[]}},\"highlight\":{\"pre_tags\":[\"@kibana-highlighted-field@\"],\"post_tags\":[\"@/kibana-highlighted-field@\"],\"fields\":{\"*\":{}},\"fragment_size\":2147483647},\"timeout\":\"30000ms\"}";

        private const string ValidHeaderContent = "{\"index\":\"kibana_sample_data_flights\",\"ignore_unavailable\":true,\"preference\":1572955935509}";
        private const string ValidSearchRequestContent = "{\"version\":true,\"size\":500,\"sort\":[{\"timestamp\":{\"order\":\"desc\",\"unmapped_type\":\"boolean\"}}],\"_source\":{\"excludes\":[]},\"aggs\":{\"2\":{\"date_histogram\":{\"field\":\"timestamp\",\"interval\":\"1d\",\"time_zone\":\"America/Los_Angeles\",\"min_doc_count\":1}}},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[{\"field\":\"timestamp\",\"format\":\"date_time\"}],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"timestamp\":{\"gte\":1561673881638,\"lte\":1566712210749,\"format\":\"epoch_millis\"}}}],\"filter\":[],\"should\":[],\"must_not\":[]}},\"highlight\":{\"pre_tags\":[\"@kibana-highlighted-field@\"],\"post_tags\":[\"@/kibana-highlighted-field@\"],\"fields\":{\"*\":{}},\"fragment_size\":2147483647},\"timeout\":\"30000ms\"}";

        private const string QueryControllerTranslateErrorString = @"
            { 
                ""responses"":[ 
                    { 
                        ""error"":{ 
                            ""root_cause"":[ 
                            { 
                                ""type"":""ArgumentException"",
                                ""reason"":""test"",
                                ""index_uuid"":""unknown"",
                                ""index"":""unknown""
                            }
                            ],
                            ""type"":""TranslateException"",
                            ""reason"":""test error message"",
                            ""phase"":""translate""
                        },
                        ""status"":500
                    }
                ]
                }
        ";

        private const string QueryControllerParseErrorString = @"
            { 
                ""responses"":[ 
                    { 
                        ""error"":{ 
                            ""root_cause"":[ 
                            { 
                                ""type"":""ArgumentException"",
                                ""reason"":""test"",
                                ""index_uuid"":""kibana_logs"",
                                ""index"":""kibana_logs""
                            }
                            ],
                            ""type"":""ParseException"",
                            ""reason"":""test error message"",
                            ""phase"":""parse""
                        },
                        ""status"":500
                    }
                ]
                }
        ";

        private const string QueryControllerQueryErrorString = @"
            { 
                ""responses"":[ 
                    { 
                        ""error"":{ 
                            ""root_cause"":[ 
                            { 
                                ""type"":""ArgumentException"",
                                ""reason"":""test"",
                                ""index_uuid"":""kibana_logs"",
                                ""index"":""kibana_logs""
                            }
                            ],
                            ""type"":""QueryException"",
                            ""reason"":""test error message"",
                            ""phase"":""query""
                        },
                        ""status"":500
                    }
                ]
                }
        ";

        private static readonly object[] InvalidQueryContent = {
            new TestCaseData(ValidHeaderContent, null, typeof(ArgumentNullException)).SetName("SearchInternal_WhenNullQuery_IsInvalid"),
            new TestCaseData(ValidHeaderContent, string.Empty, typeof(ArgumentException)).SetName("SearchInternal_WhenEmptyQuery_IsInvalid"),
            new TestCaseData(null, ValidSearchRequestContent, typeof(ArgumentNullException)).SetName("SearchInternal_WhenNullHeader_IsInvalid"),
            new TestCaseData(string.Empty, ValidSearchRequestContent, typeof(ArgumentException)).SetName("SearchInternal_WhenEmptyHeader_IsInvalid"),
        };

        private static readonly object[] IntegrationTestCases = {
            new TestCaseData(ValidMSearchRequestContent, typeof(OkObjectResult)).SetName("QueryController_WhenMSearchRequestIsValid_ReturnsOk"),
            new TestCaseData(string.Empty, typeof(BadRequestObjectResult)).SetName("QueryController_WhenMSearchRequestIsEmpty_ReturnsBadRequest"),
        };

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
        public async Task SearchInternalAsync_WithValidRequestData_ReturnsOkActionResult()
        {
            // Arrange
            var uat = GetController();

            // Act
            var result = await uat.SearchInternalAsync(ValidHeaderContent, ValidSearchRequestContent, It.IsAny<RequestContext>());

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsInstanceOf<ElasticResponse>(((OkObjectResult)result).Value);
        }

        [Test]
        public async Task SearchInternalAsync_WithValidRequestData_RunsTranslateExecuteAndParse()
        {
            // Arrange
            var queryData = new QueryData(ValidHeaderContent, ValidSearchRequestContent);
            var ts = new TimeSpan(1);
            var reader = new Mock<IDataReader>();

            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(translator => translator.TranslateQuery(ValidHeaderContent, ValidSearchRequestContent)).Returns(queryData);

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

            // Act
            await uat.SearchInternalAsync(ValidHeaderContent, ValidSearchRequestContent, It.IsAny<RequestContext>());

            // Assert
            mockTranslator.Verify(
                translator => translator.TranslateQuery(ValidHeaderContent, ValidSearchRequestContent), Times.Once());
            mockQueryExecutor.Verify(
                 executor => executor.ExecuteQueryAsync(queryData, It.IsAny<RequestContext>()), Times.Once());
            mockResponseParser.Verify(
                parsr => parsr.Parse(reader.Object, queryData, ts), Times.Once());
        }

        [TestCaseSource(nameof(InvalidQueryContent))]
        public void SearchInternalAsync_WithInvalidRequestData_ThrowsArgumentException(string header, string query, Type exceptionType)
        {
            // Arrange
            var uat = GetController();

            // Assert
            Assert.ThrowsAsync(exceptionType, async () => await uat.SearchInternalAsync(header, query, It.IsAny<RequestContext>()));
        }

        [TestCaseSource(nameof(IntegrationTestCases))]
        public async Task MultiSearchAsync_WithValidRequest_ReturnsExpectedResult(string content, Type resultType)
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
            var result = await controller.MultiSearchAsync(It.IsAny<RequestContext>());

            // Assert
            Assert.IsInstanceOf(resultType, result, $"result {result} is not of expected type {resultType.Name}");
        }

        [Test]
        public async Task MultiSearchAsync_WithErrorInTranslator_ReturnsError()
        {
            // Arrange
            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(translate
                => translate.TranslateQuery(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Throws(new TranslateException(
                    "test error message",
                    new ArgumentException("test")));
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            var mockResponseParser = new Mock<IResponseParser>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(ValidMSearchRequestContent));

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
            var result = await controller.MultiSearchAsync(It.IsAny<RequestContext>());

            // Assert
            Assert.IsInstanceOf(typeof(OkObjectResult), result, $"result {result} is not of expected type OkObjectResult");
            var asOkResult = (OkObjectResult)result;
            var resultValue = asOkResult.Value;
            resultValue.AssertJson(QueryControllerTranslateErrorString);
        }

        [Test]
        [Ignore("This needs to be enabled when the controller wraps parse errors again")]
        public async Task MultiSearchAsync_WithErrorInParser_ReturnsError()
        {
            // Arrange
            var mockQueryData = new QueryData("query", "kibana_logs");

            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(x => x.TranslateQuery(It.IsAny<string>(), It.IsAny<string>())).Returns(mockQueryData);

            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            mockResponseParser.Setup(
                parser => parser.Parse(
                        It.IsAny<IDataReader>(),
                        It.IsAny<QueryData>(),
                        It.IsAny<TimeSpan>()))
                .Throws(new ParseException(
                    "test error message",
                    new ArgumentException("test")));
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            var httpContext = new DefaultHttpContext();

            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(ValidMSearchRequestContent));

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
            var result = await controller.MultiSearchAsync(It.IsAny<RequestContext>());

            // Assert
            Assert.IsInstanceOf(typeof(OkObjectResult), result, $"result {result} is not of expected type OkObjectResult");
            var asOkResult = (OkObjectResult)result;
            var resultValue = asOkResult.Value;
            resultValue.AssertJson(QueryControllerParseErrorString);
        }

        [Test]
        public async Task MultiSearchAsync_WithErrorInExecutor_ReturnsError()
        {
            // Arrange
            var mockQueryData = new QueryData("query", "kibana_logs");
            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(x => x.TranslateQuery(It.IsAny<string>(), It.IsAny<string>())).Returns(mockQueryData);
            var mockLogger = new Mock<ILogger<QueryController>>();
            var mockResponseParser = new Mock<IResponseParser>();
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            mockQueryExecutor.Setup(executor
                => executor.ExecuteQueryAsync(
                    It.IsAny<QueryData>(),
                    It.IsAny<RequestContext>()))
                .Throws(new QueryException(
                    "test error message",
                    new ArgumentException("test")));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(ValidMSearchRequestContent));

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
            var result = await controller.MultiSearchAsync(It.IsAny<RequestContext>());

            // Assert
            Assert.IsInstanceOf(typeof(OkObjectResult), result, $"result {result} is not of expected type OkObjectResult");
            var asOkResult = (OkObjectResult)result;
            var resultValue = asOkResult.Value;
            resultValue.AssertJson(QueryControllerQueryErrorString);
        }

        [Test]
        public void SingleSearchAsync_WithInvalidIndexName_ReturnsError()
        {
            var uat = GetController();

            Assert.ThrowsAsync(typeof(ArgumentNullException), async () => await uat.SingleSearchAsync(null, It.IsAny<RequestContext>()));
            Assert.ThrowsAsync(typeof(ArgumentException), async () => await uat.SingleSearchAsync(string.Empty, It.IsAny<RequestContext>()));
        }

        private QueryController GetController()
        {
            var mockQueryData = new QueryData("query", "kibana_logs");
            var mockTranslator = new Mock<ITranslator>();
            mockTranslator.Setup(x => x.TranslateQuery(It.IsAny<string>(), It.IsAny<string>())).Returns(mockQueryData);
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

            return ctr;
        }
    }
}