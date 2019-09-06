namespace K2Bridge
{
    using System;
    using System.Linq;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(QueryStringQuery queryStringQuery)
        {
            // very basic - we should parser the phrase first
            queryStringQuery.KQL = $"search {queryStringQuery.Phrase} | project-away $table";
        }
    }
}
