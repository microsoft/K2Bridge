// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Controllers
{
    using System.Threading.Tasks;
    using global::K2Bridge.Controllers;
    using global::K2Bridge.DAL;
    using global::K2Bridge.Models;
    using global::K2Bridge.Models.Response.Metadata;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    public class IndexListControllerTests
    {
        [Test]
        public async Task IndexListController_WithValidInput_ReturnsOk()
        {
            // Arrange
            var ctr = GetController();

            // Act
            var result = await ctr.Process("testIndexName");

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        private IndexListController GetController()
        {
            var mockDAL = new Mock<IKustoDataAccess>();
            var response = new FieldCapabilityResponse();
            response.AddField(new FieldCapabilityElement { Name = "testFieldName" });
            var responseTask = Task.FromResult(response);
            mockDAL.Setup(kusto => kusto.GetFieldCapsAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>())).Returns(responseTask);
            mockDAL.Setup(kusto => kusto.GetIndexListAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>())).Returns(Task.FromResult(new IndexListResponseElement() { }));
            var mockLogger = new Mock<ILogger<IndexListController>>();

            return new IndexListController(mockDAL.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };
        }
    }
}
