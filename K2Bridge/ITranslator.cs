namespace K2Bridge
{
    internal interface ITranslator
    {
        QueryData Translate(string header, string query);
    }
}