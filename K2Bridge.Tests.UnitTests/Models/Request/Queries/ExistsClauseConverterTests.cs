// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Models.Request.Queries
{
    using System;
    using K2Bridge.Models.Request.Queries;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class ExistsClauseConverterTests
    {
        private const string FieldName = "FieldName";
        private const string DynamicFieldName = "FieldName.@1.b";
        private static readonly object[] BadInputCases =
        {
            "{ \"field\": [\"FieldName\" , \"OtherField\"] }",
            "{ \"field\": [\"FieldName\"] , \"field2\": [\"FieldName2\"] }",
            "{ \"field\": [\"FieldName\"] }",
        };

        private readonly string validExistsClause = $"{{ \"field\": \"{FieldName}\" }}";
        private readonly string validExistsClauseDynamic = $"{{ \"field\": \"{DynamicFieldName}\" }}";
        private readonly string errorMessage = $"{nameof(FieldName)} was not parsed correctly";

        [Test]
        public void DeserializeObject_WithValidInput_ReturnsObjectWithCorrectFieldName()
        {
            // Act
            var parsed = JsonConvert.DeserializeObject<ExistsClause>(validExistsClause);

            // Assert
            Assert.IsInstanceOf<ExistsClause>(parsed);
            Assert.AreEqual(FieldName, parsed.FieldName, errorMessage);
        }

        [Test]
        public void DeserializeObject_WithValidInput_ReturnsObjectWithCorrectFieldNameDynamic()
        {
            // Act
            var parsed = JsonConvert.DeserializeObject<ExistsClause>(validExistsClauseDynamic);

            // Assert
            Assert.IsInstanceOf<ExistsClause>(parsed);
            Assert.AreEqual(DynamicFieldName, parsed.FieldName, errorMessage);
        }


        [TestCaseSource(nameof(BadInputCases))]
        public void DeserializeObject_WithInvalidInput_ThrowsInvalidCastException(string input)
        {
            Assert.Throws<InvalidCastException>(() => JsonConvert.DeserializeObject<ExistsClause>(input));
        }
    }
}
