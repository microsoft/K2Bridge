using System;
using System.Collections.Generic;
using System.Data;
using K2Bridge.KustoConnector;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Linq;
using Newtonsoft.Json.Linq;
using K2Bridge.Models.Response;
using K2Bridge;

namespace Tests
{
    [TestFixture]
    public partial class ElasticResponseTests
    {
        private const string HIT_TEST_ID = "999";
        private QueryData query = new QueryData("_kql", "_index", null);

        [TestCase(ExpectedResult = "{\"responses\":[{\"took\":1,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":0,\"max_score\":null,\"hits\":[]},\"aggregations\":{\"2\":{\"buckets\":[]}},\"status\":200}]}")]
        public string DefaultResponseHasExpectedElasticProperties()
        {
            var defaultResponse = new ElasticResponse();
            var serializedResponse = JsonConvert.SerializeObject(defaultResponse);

            return serializedResponse;
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{},\"highlight\":{}}")]
        public string ResponseWithEmptyHitHasExpectedElasticProperties()
        {

            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                    }
                });
            var response = reader.ReadHits(query);

            return JsonConvert.SerializeObject(SetRandomProperties(response).First());
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"},\"highlight\":{}}")]
        public string ResponseWithSingleHitHasHasAllFieldsInSource()
        {

            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"somefield1", "somevalue1"},
                        {"somefield2", "somevalue2"},
                        {"somefield3", "somevalue3"}
                    }
                });
            var response = reader.ReadHits(query);
            return JsonConvert.SerializeObject(SetRandomProperties(response).First());
        }

        [TestCase(ExpectedResult = new[]{
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"},\"highlight\":{}}", 
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield4\":\"somevalue4\",\"somefield5\":\"somevalue5\",\"somefield6\":\"somevalue6\"},\"highlight\":{}}"})]
        public string[] ResponseWithMultipleHitHasHasAllFieldsInSource()
        {
            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"somefield1", "somevalue1"},
                        {"somefield2", "somevalue2"},
                        {"somefield3", "somevalue3"}
                    },
                    new Dictionary<string, object>{
                        {"somefield4", "somevalue4"},
                        {"somefield5", "somevalue5"},
                        {"somefield6", "somevalue6"}
                    }
            });
            var response = reader.ReadHits(query);

            return SetRandomProperties(response).Select(r =>JsonConvert.SerializeObject(r)).ToArray();
        }


        [TestCase(ExpectedResult = JTokenType.Float)]
        public JTokenType TestDecimalAreReadByType()
        {
            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"somefield1", (decimal)2}
                    }
            });
            var response = reader.ReadHits(query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Boolean)]
        public JTokenType TestBooleanAreReadBySByteType()
        {
            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"somefield1", (sbyte)0}
                    }
            });
            var response = reader.ReadHits(query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Boolean)]
        public JTokenType TestBooleanAreReadByBooleanType()
        {
            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"somefield1", false}
                    }
            });
            var response = reader.ReadHits(query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Integer)]
        public JTokenType TestIntegerAreReadType()
        {
            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"somefield1", 1}
                    }
            });
            var response = reader.ReadHits(query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Date)]
        public JTokenType TestDateAreReadType()
        {
            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"somefield1", DateTime.Now}
                    }
            });
            var response = reader.ReadHits(query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.String)]
        public JTokenType TestStringsAreReadType()
        {
            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"somefield1", "somevalue1"}
                    }
            });
            var response = reader.ReadHits(query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = JTokenType.Null)]
        public JTokenType TestDbNullAreRead()
        {
            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                        {"somefield1", DBNull.Value}
                    }
            });
            var response = reader.ReadHits(query);
            return response.First().Source.GetValue("somefield1").Type;
        }

        [TestCase(ExpectedResult = false)]
        public bool TestHitIdsAreUnique()
        {
            var reader = new TestDataReader(
                    new List<Dictionary<string, object>>(){
                        new Dictionary<string, object>{
                            {"somefield1", "somevalue1"}
                        },
                        new Dictionary<string, object>{
                            {"somefield2", "somevalue2"}
                        } ,
                        new Dictionary<string, object>{
                            {"somefield3", "somevalue3"}
                        },
                        new Dictionary<string, object>{
                            {"somefield4", "somevalue4"}
                        } ,
                        new Dictionary<string, object>{
                            {"somefield5", "somevalue5"}
                        },
                        new Dictionary<string, object>{
                            {"somefield6", "somevalue6"}
                        }                    
                });
            
            var response = reader.ReadHits(query);
            var hash = new Dictionary<string, int>();
            foreach(var hit in response){
                if (hash.ContainsKey(hit.Id))
                    return true;
                hash.Add(hit.Id, 1);
            }
            return false;
        }

        private IEnumerable<Hit> SetRandomProperties(IEnumerable<Hit> hits) =>  hits.Select(i =>{ i.Id = HIT_TEST_ID; return i;});

    }    
}