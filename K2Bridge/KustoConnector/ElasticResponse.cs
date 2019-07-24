using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace K2Bridge.KustoConnector
{
    public class ElasticResponse
    {
        public Response[] responses { get; set; }
    }

    public class Response
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public _Shards _shards { get; set; }
        public Hits hits { get; set; }
        public Aggregations aggregations { get; set; }
        public int status { get; set; }
    }

    public class _Shards
    {
        public int total { get; set; }
        public int successful { get; set; }
        public int skipped { get; set; }
        public int failed { get; set; }
    }

    public class Hits
    {
        public int total { get; set; }
        public object max_score { get; set; }
        public Hit[] hits { get; set; }
    }

    public class Hit
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public int _version { get; set; }
        public object _score { get; set; }
        public _Source _source { get; set; }
        public Fields fields { get; set; }
        public long[] sort { get; set; }
    }

    public class _Source
    {
        public string FlightNum { get; set; }
        public string DestCountry { get; set; }
        public string OriginWeather { get; set; }
        public string OriginCityName { get; set; }
        public float AvgTicketPrice { get; set; }
        public float DistanceMiles { get; set; }
        public bool FlightDelay { get; set; }
        public string DestWeather { get; set; }
        public string Dest { get; set; }
        public string FlightDelayType { get; set; }
        public string OriginCountry { get; set; }
        public int dayOfWeek { get; set; }
        public float DistanceKilometers { get; set; }
        public object timestamp { get; set; }
        public Destlocation DestLocation { get; set; }
        public string DestAirportID { get; set; }
        public string Carrier { get; set; }
        public bool Cancelled { get; set; }
        public float FlightTimeMin { get; set; }
        public string Origin { get; set; }
        public Originlocation OriginLocation { get; set; }
        public string DestRegion { get; set; }
        public string OriginAirportID { get; set; }
        public string OriginRegion { get; set; }
        public string DestCityName { get; set; }
        public float FlightTimeHour { get; set; }
        public int FlightDelayMin { get; set; }
    }

    public class Destlocation
    {
        public string lat { get; set; }
        public string lon { get; set; }
    }

    public class Originlocation
    {
        public string lat { get; set; }
        public string lon { get; set; }
    }

    public class Fields
    {
        public int[] hour_of_day { get; set; }
        public DateTime[] timestamp { get; set; }
    }

    public class Aggregations
    {
        [JsonProperty(PropertyName = "2")]
        public _2 _2 { get; set; }
    }

    public class _2
    {
        public Bucket[] buckets { get; set; }
    }

    public class Bucket
    {
        public DateTime key_as_string { get; set; }
        public long key { get; set; }
        public int doc_count { get; set; }
    }

}
