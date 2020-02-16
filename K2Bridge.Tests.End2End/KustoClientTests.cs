// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System.Threading.Tasks;
    using FluentAssertions.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    public class KustoClientTests : KustoTestBase
    {
        [Test]
        [Description("MSearch returns data a functions")]
        public async Task MSearch_All_ReturnsAllHitsAsExpected()
        {
            var expected = JObject.Parse(@"{
                    ""_source"": {
                        ""timestamp"": ""2020-02-03T12:53:12.6309627"",
                        ""Name"": ""sds"",
                        ""Dec"": ""NaN"",
                        ""TimeSpan"": null,
                        ""SomeID"": null
                    }
                }");
            var result = await K2Client().MSearch(TYPESINDEX, $"{TYPESDIR}/MSearch_All_InTimeRange.json");
            var totalHits = result.SelectToken("responses[0].hits.total");
            Assert.IsNotNull(totalHits);
            Assert.IsTrue(totalHits.Value<int>() == 5);
            var hits = result.SelectToken("responses[0].hits.hits[0]");
            result.Should().ContainSubtree(expected);
        }
    }
}
