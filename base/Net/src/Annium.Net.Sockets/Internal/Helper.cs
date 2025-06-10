using System;
using System.IO;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Helper class for creating managed socket instances based on mode
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Creates a managed socket instance based on the specified mode
    /// </summary>
    /// <param name="stream">The network stream</param>
    /// <param name="options">Configuration options including socket mode</param>
    /// <param name="logger">Logger instance for diagnostics</param>
    /// <returns>A managed socket instance appropriate for the specified mode</returns>
    /// <exception cref="InvalidOperationException">Thrown when socket mode is not supported</exception>
    public static IManagedSocket GetManagedSocket(Stream stream, ManagedSocketOptions options, ILogger logger) =>
        options.Mode switch
        {
            SocketMode.Raw => new RawManagedSocket(stream, options, logger),
            SocketMode.Messaging => new MessagingManagedSocket(stream, options, logger),
            _ => throw new InvalidOperationException($"Unexpected socket mode {options.Mode}"),
        };
}
