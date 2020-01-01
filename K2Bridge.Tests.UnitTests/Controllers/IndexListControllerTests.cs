// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Tests.UnitTests.Controllers
{
    using System.Threading.Tasks;
    using K2Bridge.Controllers;
    using K2Bridge.DAL;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    public class IndexListControllerTests
    {
        private IndexListController GetController()
        {
            var mockDAL = new Mock<IKustoDataAccess>();
            var reponse = new Models.Response.Metadata.FieldCapabilityResponse();
            reponse.AddField(new Models.Response.Metadata.FieldCapabilityElement { Name = "testFieldName" });
            mockDAL.Setup(kusto => kusto.GetFieldCaps(It.IsNotNull<string>())).Returns(reponse);
            mockDAL.Setup(kusto => kusto.GetIndexList(It.IsNotNull<string>())).Returns(new Models.Response.Metadata.IndexListResponseElement() { });
            var mockLogger = new Mock<ILogger<IndexListController>>();

            return new IndexListController(mockDAL.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Test]
        public async Task WhenCorrectInputReturnOkResult()
        {
            // Arrange
            var ctr = GetController();

            // Act
            var result = await ctr.Process();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}
