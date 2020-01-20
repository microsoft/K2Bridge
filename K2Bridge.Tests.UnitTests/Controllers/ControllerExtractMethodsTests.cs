// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2BridgeUnitTests
{
    using System;
    using K2Bridge.Controllers;
    using NUnit.Framework;

    [TestFixture]
    public class ControllerExtractMethodsTests
    {
        private static readonly object[] PartitionStringTests = {
            new TestCaseData("header\r\nquery").Returns(("header", "query")).SetName("PartitionFunc_SlashNSlashR_PartitionsStrings"),
            new TestCaseData("header\rquery").Returns(("header", "query")).SetName("PartitionFunc_SlashR_PartitionsStrings"),
            new TestCaseData("header\nquery").Returns(("header", "query")).SetName("PartitionFunc_SlashN_PartitionsStrings"),
            new TestCaseData("headerquery").Returns(ValueTuple.Create<string, string>("headerquery", null)).SetName("PartitionFunc_WhenNoCharacters_DoesNotPartitionsStrings"),
            new TestCaseData("header\nquery\nnotheader\nnotquery").Returns(("header", "query")).SetName("PartitionFunc_MorePartitions_Ignores"),
            new TestCaseData("header").Returns(ValueTuple.Create<string, string>("header", null)).SetName("PartitionFunc_LessPartitions_NoError"),
        };

        [TestCaseSource("PartitionStringTests")]
        public (string, string) ExtractHeaderQueryTests(string input)
        {
            return ControllerExtractMethods.SplitQueryBody(input);
        }
    }
}
