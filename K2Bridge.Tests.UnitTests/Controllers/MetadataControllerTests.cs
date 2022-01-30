// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Controllers;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using K2Bridge;
using K2Bridge.Controllers;
using K2Bridge.HttpMessages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;

[TestFixture]
public class MetadataControllerTests
{
    private const string ValidQueryPath = "/_template/kibana_index_template::kibana?include_type_name=true";

    private static readonly object[] IntegrationTestCases = {
            new TestCaseData("/_template/kibana_index_template::kibana?include_type_name=true", "POST", typeof(HttpResponseMessageResult)).Returns(string.Empty).SetName("PassThroughController_WhenQueryIsValidPost_ReturnsOk"),
            new TestCaseData("/_template/kibana_index_template::kibana?include_type_name=true", "HEAD", typeof(HttpResponseMessageResult)).Returns(string.Empty).SetName("PassThroughController_WhenQueryIsValidHead_ReturnsOk"),
            new TestCaseData("/_template/kibana_index_template::kibana?include_type_name=true", "PUT", typeof(HttpResponseMessageResult)).Returns(string.Empty).SetName("PassThroughController_WhenQueryIsValidPut_ReturnsOk"),
            new TestCaseData("/_template/kibana_index_template::kibana?include_type_name=true", "PATCH", typeof(HttpResponseMessageResult)).Returns(string.Empty).SetName("PassThroughController_WhenQueryIsValidPatch_ReturnsOk"),
            new TestCaseData("/_template/kibana_index_template::kibana?include_type_name=true", "OPTIONS", typeof(HttpResponseMessageResult)).Returns(string.Empty).SetName("PassThroughController_WhenQueryIsValidOptions_ReturnsOk"),
            new TestCaseData("/_template/kibana_index_template::kibana?include_type_name=true", "GET", typeof(HttpResponseMessageResult)).Returns(string.Empty).SetName("PassThroughController_WhenQueryIsValidGet_ReturnsOk"),
            new TestCaseData("/_template/kibana_index_template::kibana?include_type_name=true", "DELETE", typeof(HttpResponseMessageResult)).Returns(string.Empty).SetName("PassThroughController_WhenQueryIsValidDelete_ReturnsOk"),
            new TestCaseData(null, "POST", typeof(BadRequestObjectResult)).Returns(string.Empty).SetName("PassThroughController_WhenQueryIsInvalid_ReturnsBadRequest"),
        };

    private static readonly object[] ReplaceStringTestCases = {
            new TestCaseData("/_template/kibana_index_template::kibana", "/_template/kibana_index_template:.kibana").SetName("PassThroughController_WhenQueryIsValid_ReplaceTokensBeforeSendingToFallback"),
        };

    [Test]
    public async Task PassThrough_OnValidInput_ReturnsAnOkHttpMessageActionFromFallback()
    {
        // Arrange
        var controllerFixture = GetControllerFixture(ValidQueryPath, "POST");

        // Act
        var result = await controllerFixture.Controller.PassthroughInternal();
        controllerFixture.Message.Dispose();
        controllerFixture.Client.Dispose();

        // Assert
        Assert.IsInstanceOf<HttpResponseMessageResult>(result);
    }

    [Test]
    public void MetadataControllerConstructor_WhenNoArgs_ThrowsOnInit()
    {
        var mockClientFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<MetadataController>>();

        Assert.Throws<ArgumentNullException>(() =>
        {
            new MetadataController(null, mockLogger.Object);
        });

        Assert.Throws<ArgumentNullException>(() =>
        {
            new MetadataController(mockClientFactory.Object, null);
        });
    }

    [TestCaseSource(nameof(IntegrationTestCases))]
    public async Task<string> PassThroughController_Integration_Tests(string input, string method, Type resultType)
    {
        Ensure.IsNotNull(resultType, nameof(resultType));

        // Arrange
        var controllerFixture = GetControllerFixture(input, method);

        // Act
        var result = await controllerFixture.Controller.Passthrough();
        controllerFixture.Message.Dispose();
        controllerFixture.Client.Dispose();

        // Assert
        Assert.IsInstanceOf(resultType, result, $"result {result} is not of expected type {resultType.Name}");
        return string.Empty;
    }

    [TestCaseSource(nameof(ReplaceStringTestCases))]
    public async Task PassThroughInternal_WhenInputContainsIllegalTokens_ReplaceBack(string input, string expectedHttpClient)
    {
        // Arrange
        var controllerFixture = GetControllerFixture(input, "POST", false);

        controllerFixture.HttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.ToString().Contains(expectedHttpClient, StringComparison.OrdinalIgnoreCase)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(controllerFixture.Message)
            .Verifiable();

        // Act
        var result = await controllerFixture.Controller.Passthrough();
        controllerFixture.Message.Dispose();
        controllerFixture.Client.Dispose();

        // Assert
        Assert.IsInstanceOf(typeof(HttpResponseMessageResult), result, $"result {result} is not of expected type HttpResponseMessageResult");
    }

    [Test]
    public async Task PassThrough_IncompleteTask_ReturnsError()
    {
        // Arrange
        var controllerFixture = GetControllerFixture("/_template/kibana_index_template::kibana", "POST", false);
        var completionSource = new TaskCompletionSource<HttpResponseMessage>();
        _ = Task.Run(() =>
          {
              Thread.Sleep(200);
              completionSource.SetResult(controllerFixture.Message);
          });
        controllerFixture.HttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(completionSource.Task)
            .Verifiable();

        // Act
        var result = await controllerFixture.Controller.Passthrough();
        controllerFixture.Message.Dispose();
        controllerFixture.Client.Dispose();

        // Assert
        Assert.IsInstanceOf(typeof(HttpResponseMessageResult), result, $"result {result} is not of expected type BadRequestObjectResult");
    }

    [Test]
    public async Task PassThrough_ErrorInHttpClient_ReturnsError()
    {
        // Arrange
        var controllerFixture = GetControllerFixture("/_template/kibana_index_template::kibana", "POST", false);
        controllerFixture.HttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Throws(new System.Web.Http.HttpResponseException(HttpStatusCode.BadRequest))
            .Verifiable();

        // Act
        var result = await controllerFixture.Controller.Passthrough();
        controllerFixture.Message.Dispose();
        controllerFixture.Client.Dispose();

        // Assert
        Assert.IsInstanceOf(typeof(BadRequestObjectResult), result, $"result {result} is not of expected type BadRequestObjectResult");
    }

    private static MetadataControllerFixture GetControllerFixture(string path, string method, bool setupHandler = true)
    {
        var mockClientFactory = new Mock<IHttpClientFactory>();
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        var message = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("[{'id':1,'value':'1'}]"),
        };
        if (setupHandler)
        {
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(message)
                .Verifiable();
        }

        var mockHttpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
        mockClientFactory.Setup(x => x.CreateClient(MetadataController.ElasticMetadataClientName)).Returns(mockHttpClient);
        var mockLogger = new Mock<ILogger<MetadataController>>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.Request.Path = path;
        httpContext.Request.PathBase = path;
        if (!string.IsNullOrEmpty(path))
        {
            httpContext.Request.Host = new HostString("www.someserver.com");
        }

        httpContext.Request.Scheme = "http";

        // Controller needs a controller context
        var controllerContext = new ControllerContext()
        {
            HttpContext = httpContext,
        };
        return new MetadataControllerFixture()
        {
            Controller = new MetadataController(mockClientFactory.Object, mockLogger.Object)
            {
                ControllerContext = controllerContext,
            },
            Message = message,
            Client = mockHttpClient,
            HttpMessageHandler = handlerMock,
        };
    }

    private struct MetadataControllerFixture
    {
        public MetadataController Controller { get; set; }

        public HttpResponseMessage Message { get; set; }

        public HttpClient Client { get; set; }

        public Mock<HttpMessageHandler> HttpMessageHandler { get; set; }
    }
}
