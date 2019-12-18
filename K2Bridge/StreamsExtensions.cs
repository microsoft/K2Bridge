// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace K2Bridge
{
    using System.IO;

    /// <summary>
    /// Streams extension methods
    /// </summary>
    internal static class StreamsExtensions
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
    }
}
