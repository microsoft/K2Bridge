using System;
using System.Collections.Generic;
using System.Data;
using K2Bridge.KustoConnector;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Linq;
using Newtonsoft.Json.Linq;
using K2Bridge.Models.Response;

namespace Tests
{
    [TestFixture]
    public partial class ElasticResponseTests
    {
        [TestCase(ExpectedResult = "{\"responses\":[{\"took\":1,\"timed_out\":false,\"_shards\":{\"total\":1,\"successful\":1,\"skipped\":0,\"failed\":0},\"hits\":{\"total\":0,\"max_score\":null,\"hits\":[]},\"aggregations\":{\"2\":{\"buckets\":[]}},\"status\":200}]}")]
        public string DefaultResponseHasExpectedElasticProperties()
        {
            var defaultResponse = new ElasticResponse();
            var serializedResponse = JsonConvert.SerializeObject(defaultResponse);

            return serializedResponse;
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{}}")]
        public string ResponseWithEmptyHitHasExpectedElasticProperties()
        {

            var reader = new TestDataReader(
                new List<Dictionary<string, object>>(){
                    new Dictionary<string, object>{
                    }
                });
            var response = reader.ReadHits("_index");

            return JsonConvert.SerializeObject(response.First());
        }

        [TestCase(ExpectedResult = "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"}}")]
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
            var response = reader.ReadHits("_index");

            return JsonConvert.SerializeObject(response.First());
        }

        [TestCase(ExpectedResult = new[]{
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield1\":\"somevalue1\",\"somefield2\":\"somevalue2\",\"somefield3\":\"somevalue3\"}}", 
            "{\"_index\":\"_index\",\"_type\":\"_doc\",\"_id\":\"999\",\"_version\":1,\"_score\":null,\"_source\":{\"somefield4\":\"somevalue4\",\"somefield5\":\"somevalue5\",\"somefield6\":\"somevalue6\"}}"})]
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
            var response = reader.ReadHits("_index");

            return response.Select(r => JsonConvert.SerializeObject(r)).ToArray();
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
            var response = reader.ReadHits("_index");
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
            var response = reader.ReadHits("_index");
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
            var response = reader.ReadHits("_index");
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
            var response = reader.ReadHits("_index");
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
            var response = reader.ReadHits("_index");
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
            var response = reader.ReadHits("_index");
            return response.First().Source.GetValue("somefield1").Type;
        }
    }    
}