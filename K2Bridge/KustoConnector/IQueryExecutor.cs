namespace K2Bridge.KustoConnector
{
    using System.Data;
    using K2Bridge.Models.Response;

    public interface IQueryExecutor
    {
        ElasticResponse ExecuteQuery(QueryData query);

        IDataReader ExecuteControlCommand(string query);
    }
}