namespace K2Bridge
{
    using System.IO;

    internal static class StreamUtils
    {
        internal static void CopyStream(Stream source, Stream destination)
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

    }
}
