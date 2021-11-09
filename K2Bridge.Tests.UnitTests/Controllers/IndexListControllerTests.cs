// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Controllers
{
    using System;
    using System.Threading.Tasks;
    using global::K2Bridge.Controllers;
    using global::K2Bridge.KustoDAL;
    using global::K2Bridge.Models.Response.Metadata;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    public class IndexListControllerTests
    {
        [Test]
        public async Task IndexListController_ResolveWithValidInput_ReturnsOk()
        {
            // Arrange
            var ctr = GetController();

            // Act
            var result = await ctr.Resolve("testIndexName");

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        private IndexListController GetController()
        {
            var mockDAL = new Mock<IKustoDataAccess>();
            var response = new FieldCapabilityResponse();
            response.AddField(new FieldCapabilityElement { Name = "testFieldName" });
            var responseTask = Task.FromResult(response);
            mockDAL.Setup(kusto => kusto.GetFieldCapsAsync(It.IsNotNull<string>())).Returns(responseTask);
            mockDAL.Setup(kusto => kusto.ResolveIndexAsync(It.IsNotNull<string>())).Returns(Task.FromResult(new ResolveIndexResponse() { }));
            var mockLogger = new Mock<ILogger<IndexListController>>();

            var ctr = new IndexListController(mockDAL.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext()
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
