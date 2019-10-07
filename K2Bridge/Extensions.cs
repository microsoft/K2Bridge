namespace K2Bridge
{
    using System.IO;
    using Newtonsoft.Json.Linq;

    internal static class Extensions
    {
        internal static void CopyStream(this Stream source, Stream destination)
        {
            if (source.CanSeek && source.Position > 0)
            {
                source.Position = 0;
            }

            source.CopyTo(destination);

            if (destination.CanSeek && destination.Position > 0)
            {
                destination.Position = 0;
            }
        }

        internal static string TokenToString(this JToken jToken)
        {
            var eObj = jToken == null ? null : jToken.Value<object>();
            return eObj == null ? string.Empty : eObj.ToString();
        }
    }
}
