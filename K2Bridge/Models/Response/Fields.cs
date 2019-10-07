namespace K2Bridge.KustoConnector
{
    using System;
    using Newtonsoft.Json;

    public class Fields
    {
        [JsonProperty("hour_of_day")]
        public int[] HourOfDay { get; set; }

        [JsonProperty("timestamp")]
        public DateTime[] Timestamp { get; set; }
    }
}
