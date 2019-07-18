namespace K2Bridge
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    class BoolClause
    {
        [JsonProperty("must")]
        public List<LeafQueryClause> Must { get; set; }

        void xyz()
        {
            LeafQueryClause leafQueryClause;
            RangeQuery rangeQuery = new RangeQuery();

            leafQueryClause = rangeQuery;
        }
    }
}
