namespace K2Bridge.Models.Request.Queries
{
    using K2Bridge.Models.Request;
    using Newtonsoft.Json;

    [JsonConverter(typeof(LeafQueryClauseConverter))]
    internal interface ILeafQueryClause : IQueryClause
    {
        string KQL { get; set; }
    }
}
