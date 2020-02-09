// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.End2End
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using FluentAssertions.Json;
    using Kusto.Data.Common;
    using Kusto.Data.Net.Client;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    public class KustoFunctionsTest : KustoTestBase
    {
        private static ICslAdminProvider kustoAdminClient;

        private static string functionFullName;

        private static string typesIndexFullName;

        [OneTimeSetUp]
        public static void CreateClient()
        {
            using (kustoAdminClient = KustoClientFactory.CreateCslAdminProvider(Kusto()))
            {
                PopulateFunctionsData();
            }

            functionFullName = $"{KustoDatabase()}:fn_countries_and_airports";

            typesIndexFullName = $"{KustoDatabase()}:{TYPESINDEX}";
        }

        [Test]
        [Description("SearchAsync (IndexList) returns functions")]
        public async Task Function_Search()
        {
            var indexList = await K2Client().Search();
            var match = indexList.SelectToken($"aggregations.indices.buckets[?(@.key == '{functionFullName}')]");
            Assert.IsNotNull(match);
        }

        [Test]
        [Description("FieldCaps returns fields for functions")]
        public async Task Function_FieldCaps()
        {
            var fieldCaps = await K2Client().FieldCaps(functionFullName);
            var expected = JObject.Parse(@"{
              ""fields"": {
                ""OriginCountry"": {
                  ""keyword"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""keyword""
                  }
                },
                ""Origin"": {
                  ""keyword"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""keyword""
                  }
                },
                ""timestamp"": {
                  ""date"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""date""
                  }
                }
              }
            }");
            fieldCaps.Should().BeEquivalentTo(expected);
        }

        [Test]
        [Description("FieldCaps returns fields for functions")]
        public async Task Function_FieldCaps_ReturnsAllTypes()
        {
            var fieldCaps = await K2Client().FieldCaps(typesIndexFullName);
            var expected = JObject.Parse(@"{
              ""fields"": {
                ""Boolean"": {
                  ""boolean"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""boolean""
                  }
                },
                ""DateTime"": {
                  ""date"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""date""
                  }
                },
                ""Guid"": {
                  ""string"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""string""
                  }
                },
                ""Int32"": {
                  ""integer"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""integer""
                  }
                },
                ""Int64"": {
                  ""long"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""long""
                  }
                },
                ""Double"": {
                  ""double"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""double""
                  }
                },
                ""String"": {
                  ""keyword"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""keyword""
                  }
                },
                ""TimeSpan"": {
                  ""string"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""string""
                  }
                },
                ""SqlDecimal"": {
                  ""double"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""double""
                  }
                },
                ""Dynamic"": {
                  ""object"": {
                    ""aggregatable"": true,
                    ""searchable"": true,
                    ""type"": ""object""
                  }
                },
              }
            }");
            fieldCaps.Should().BeEquivalentTo(expected);
        }

        [Test]
        [Description("MSearch returns data from functions")]
        public async Task Function_MSearch()
        {
            var result = await K2Client().MSearch(functionFullName, $"{FLIGHTSDIR}/MSearch_TwoResults_Equivalent.json");
            var totalHits = result.SelectToken("responses[0].hits.total");
            Assert.IsNotNull(totalHits);
            Assert.IsTrue(totalHits.Value<int>() == 2);
            var indexName = result.SelectToken("responses[0].hits.hits[0]._index");
            Assert.IsNotNull(indexName);
            Assert.AreEqual($"{KustoDatabase()}:fn_countries_and_airports", indexName.Value<string>());
        }

        private static void PopulateFunctionsData()
        {
            // Create `countries` table to test an ADX join query
            KustoExecute(@"
                .set-or-replace countries <| 
                  print CountryCode='AE', CountryName='United Arab Emirates'
                  | union (print CountryCode='AR', CountryName='Argentina')
                  | union (print CountryCode='AT', CountryName='Austria')
                  | union (print CountryCode='AU', CountryName='Australia')
                  | union (print CountryCode='CA', CountryName='Canada')
                  | union (print CountryCode='CH', CountryName='Switzerland')
                  | union (print CountryCode='CL', CountryName='Chile')
                  | union (print CountryCode='CN', CountryName='China')
                  | union (print CountryCode='CO', CountryName='Colombia')
                  | union (print CountryCode='DE', CountryName='Germany')
                  | union (print CountryCode='DK', CountryName='Denmark')
                  | union (print CountryCode='EC', CountryName='Ecuador')
                  | union (print CountryCode='ES', CountryName='Spain')
                  | union (print CountryCode='FI', CountryName='Finland')
                  | union (print CountryCode='FR', CountryName='France')
                  | union (print CountryCode='GB', CountryName='United Kingdom')
                  | union (print CountryCode='IE', CountryName='Ireland')
                  | union (print CountryCode='IN', CountryName='India')
                  | union (print CountryCode='IT', CountryName='Italy')
                  | union (print CountryCode='JP', CountryName='Japan')
                  | union (print CountryCode='KR', CountryName='Korea')
                  | union (print CountryCode='MX', CountryName='Mexico')
                  | union (print CountryCode='NL', CountryName='Netherlands')
                  | union (print CountryCode='NO', CountryName='Norway')
                  | union (print CountryCode='PE', CountryName='Peru')
                  | union (print CountryCode='PL', CountryName='Poland')
                  | union (print CountryCode='PR', CountryName='Puerto Rico')
                  | union (print CountryCode='RU', CountryName='Russian Federation')
                  | union (print CountryCode='SE', CountryName='Sweden')
                  | union (print CountryCode='TR', CountryName='Turkey')
                  | union (print CountryCode='US', CountryName='United States')
                  | union (print CountryCode='ZA', CountryName='South Africa')
                  ");

            // Drop functions if they exist
            KustoExecute(@".drop functions (fn_countries_and_airports, fn_with_params) ifexists");

            // Function under test performing a join between two tables
            KustoExecute(@".create function fn_countries_and_airports() {kibana_sample_data_flights | join (countries) on $left.OriginCountry == $right.CountryCode | project timestamp, OriginCountry, Origin }");

            // Another function that takes parameters - should not be surfaced
            KustoExecute(@".create function fn_with_params(myLimit: long) {kibana_sample_data_flights | limit myLimit }");
        }

        private static void KustoExecute(string command)
        {
            TestContext.Progress.WriteLine(command);
            kustoAdminClient.ExecuteControlCommand(command);
        }
    }
}
