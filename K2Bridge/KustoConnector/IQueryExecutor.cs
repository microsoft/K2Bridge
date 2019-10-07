namespace K2Bridge.KustoConnector
{
    using System.Data;

    public interface IQueryExecutor
    {
        ElasticResponse ExecuteQuery(QueryData query);

        IDataReader ExecuteControlCommand(string query);
    }
}