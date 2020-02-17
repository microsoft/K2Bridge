// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Threading.Tasks;
    using global::K2Bridge.KustoDAL;
    using global::K2Bridge.Models;
    using global::K2Bridge.Models.Response.Metadata;
    using Microsoft.Extensions.Logging;
    using Moq;

    public static class SchemaRetrieverMock
    {
        public static ISchemaRetrieverFactory CreateMockSchemaRetriever()
        {
            var response = new FieldCapabilityResponse();
            response.AddField(
                new FieldCapabilityElement
                {
                    Name = "dayOfWeek",
                    Type = "string",
                });
            var responseTask = Task.FromResult(response);

            var mockDAL = new Mock<IKustoDataAccess>();
            mockDAL.Setup(kusto => kusto.GetFieldCapsAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>())).Returns(responseTask);

            var mockLogger = new Mock<ILogger<SchemaRetriever>>();
            return new SchemaRetrieverFactory(mockLogger.Object, mockDAL.Object);
        }

        public static ISchemaRetrieverFactory CreateMockNumericSchemaRetriever()
        {
            var response = new FieldCapabilityResponse();
            response.AddField(
                new FieldCapabilityElement
                {
                    Name = "dayOfWeek",
                    Type = "long",
                });
            var responseTask = Task.FromResult(response);

            var mockDAL = new Mock<IKustoDataAccess>();
            mockDAL.Setup(kusto => kusto.GetFieldCapsAsync(It.IsNotNull<string>(), It.IsAny<RequestContext>())).Returns(responseTask);

            var mockLogger = new Mock<ILogger<SchemaRetriever>>();
            return new SchemaRetrieverFactory(mockLogger.Object, mockDAL.Object);
        }
    }
}