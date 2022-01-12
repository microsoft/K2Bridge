// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.JsonConverters
{
    using global::K2Bridge.Models.Response.Metadata;
    using NUnit.Framework;

    [TestFixture]
    public class FieldcapsConverterTests
    {
        private const string ExpectedValidFieldCaps = @"
            { 
            ""text"": {
                ""aggregatable"": true,
                ""searchable"": true,
                ""metadata_field"": false,
                ""type"": ""text""
            }
        }";

        private static readonly FieldCapabilityElement ValidFieldCapsElement = new FieldCapabilityElement()
        {
            Name = "title",
            Type = "text",
            IsAggregatable = true,
            IsSearchable = true,
            IsMetadataField = false,
        };

        private static readonly object[] FieldCapsTestCases = {
            new TestCaseData(ExpectedValidFieldCaps, ValidFieldCapsElement).SetName("JsonDeserialize_WithValidFieldCaps_DeserializedCorrectly"),
        };

        [TestCaseSource(nameof(FieldCapsTestCases))]
        public void TestFieldCapsConverter(string queryString, object expected)
        {
            ((FieldCapabilityElement)expected).AssertJson(queryString);
        }
    }
}