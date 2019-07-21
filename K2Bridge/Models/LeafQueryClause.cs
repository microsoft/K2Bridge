namespace K2Bridge
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(LeafQueryClauseConverter))]
    internal abstract class LeafQueryClause : KQLBase
    {
    }
}
