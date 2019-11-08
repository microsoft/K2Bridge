namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request.Aggregations;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        private static void ValidateField(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new IllegalClauseException("Average FieldName must have a valid value");
            }
        }

        public void Visit(Avg avg)
        {
            ValidateField(avg.FieldName);

            avg.KQL = $"{KQLOperators.Avg}({avg.FieldName})";
        }

        public void Visit(Cardinality cardinality)
        {
            ValidateField(cardinality.FieldName);

            cardinality.KQL = $"{KQLOperators.DCount}({cardinality.FieldName})";
        }
    }
}
