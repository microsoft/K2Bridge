namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;

    [JsonConverter(typeof(LeafQueryClauseConverter))]
    internal abstract class LeafQueryClause : KQLBase, IQueryClause
    {
    }
}
