using System;
using System.IO;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

internal static class Helper
{
    public static IManagedSocket GetManagedSocket(SocketMode socketMode, Stream stream, ILogger logger) =>
        socketMode switch
        {
            SocketMode.Raw => new RawManagedSocket(stream, logger),
            SocketMode.Messaging => new MessagingManagedSocket(stream, logger),
            _ => throw new InvalidOperationException($"Unexpected socket mode {socketMode}")
        };
}
