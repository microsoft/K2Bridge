namespace K2Bridge
{
    using Newtonsoft.Json;

    [JsonConverter(typeof(SortClauseConverter))]
    internal class SortClause : KQLBase, IVisitable
    {
        public string FieldName { get; set; }

        public string Order { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
