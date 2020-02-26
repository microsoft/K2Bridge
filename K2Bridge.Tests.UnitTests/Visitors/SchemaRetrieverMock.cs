﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Threading.Tasks;
    using global::K2Bridge.KustoDAL;
    using global::K2Bridge.Models.Response.Metadata;
    using Microsoft.Extensions.Logging;
    using Moq;

    public static class SchemaRetrieverMock
    {
        public static ISchemaRetrieverFactory CreateMockSchemaRetriever(string name = "dayOfWeek", string type = "string")
        {
            var response = new FieldCapabilityResponse();
            response.AddField(
                new FieldCapabilityElement
                {
                    Name = name,
                    Type = type,
                });
            var responseTask = Task.FromResult(response);

            var mockDAL = new Mock<IKustoDataAccess>();
            mockDAL.Setup(kusto => kusto.GetFieldCapsAsync(It.IsNotNull<string>())).Returns(responseTask);

            var mockLogger = new Mock<ILogger<SchemaRetriever>>();
            return new SchemaRetrieverFactory(mockLogger.Object, mockDAL.Object);
        }

        public static ISchemaRetrieverFactory CreateMockNumericSchemaRetriever(string name = "dayOfWeek", string type = "long")
        {
            return CreateMockSchemaRetriever(name, type);
        }

        public static ISchemaRetrieverFactory CreateMockDateSchemaRetriever(string name = "MyField", string type = "date")
        {
            return CreateMockSchemaRetriever(name, type);
        }
    }
}