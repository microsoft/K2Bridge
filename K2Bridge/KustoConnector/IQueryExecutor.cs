namespace K2Bridge.KustoConnector
{
    public interface IQueryExecutor
    {
        ElasticResponse ExecuteQuery(string query);
    }
}