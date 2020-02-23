// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    public class KustoClientTests : KustoTestBase
    {
        [Test]
        [Description("MSearch returns data a functions")]
        public async Task MSearch_All_ReturnsAllHitsAsExpected()
        {
            var result = await K2Client().MSearch(TYPESINDEX, $"{TYPESDIR}/MSearch_All_InTimeRange.json");
            var totalHits = result.SelectToken("responses[0].hits.total");
            Assert.IsNotNull(totalHits);
            Assert.IsTrue(totalHits.Value<int>() == 3);
        }
    }
}
