// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Controllers
{
    using System;
    using System.Threading.Tasks;
    using global::K2Bridge.Controllers;
    using global::K2Bridge.KustoDAL;
    using global::K2Bridge.Models;
    using global::K2Bridge.Models.Response.Metadata;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    public class FieldCapabilityControllerTests
    {
        public FieldCapabilityControllerTests()
        {
        }

        [Test]
        public async Task FieldCaps_WithValidInit_ReturnsValidResult()
        {
            // Arrange
            var ctr = GetController();

            // Act
            var result = await ctr.Process("indexname");

            // Assert
            Assert.IsInstanceOf<ContentResult>(result);
        }

        private FieldCapabilityController GetController()
        {
            var mockDAL = new Mock<IKustoDataAccess>();
            var response = new FieldCapabilityResponse();
            response.AddField(new FieldCapabilityElement { Name = "testFieldName" });
            var responseTask = Task.FromResult(response);
            mockDAL.Setup(kusto => kusto.GetFieldCapsAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>())).Returns(responseTask);
            var mockLogger = new Mock<ILogger<FieldCapabilityController>>();

            var ctr = new FieldCapabilityController(mockDAL.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            const string correlationIdHeader = "x-correlation-id";
            ctr.HttpContext.Request.Headers[correlationIdHeader] = Guid.NewGuid().ToString();

            return ctr;
        }
    }
}
