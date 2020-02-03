// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Controllers
{
    using System.Threading.Tasks;
    using K2Bridge.Controllers;
    using K2Bridge.DAL;
    using K2Bridge.Models.Response.Metadata;
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
        public async Task WhenCorrectInputReturnContentResult()
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
            mockDAL.Setup(kusto => kusto.GetFieldCapsAsync(It.IsNotNull<string>())).Returns(responseTask);
            var mockLogger = new Mock<ILogger<FieldCapabilityController>>();

            return new FieldCapabilityController(mockDAL.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };
        }
    }
}
