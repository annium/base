using System;
using System.IO;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

internal static class Helper
{
    public static IManagedSocket GetManagedSocket(Stream stream, ManagedSocketOptions options, ILogger logger) =>
        options.Mode switch
        {
            SocketMode.Raw => new RawManagedSocket(stream, options, logger),
            SocketMode.Messaging => new MessagingManagedSocket(stream, options, logger),
            _ => throw new InvalidOperationException($"Unexpected socket mode {options.Mode}"),
        };
}
