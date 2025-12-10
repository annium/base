using System;

namespace Annium.Net.Servers.Sockets;

/// <summary>
/// Defines a contract for a socket server that can be started and run asynchronously
/// </summary>
public interface IServer : IAsyncDisposable
{
    /// <summary>
    /// Host, server is listening at
    /// </summary>
    string Host { get; }

    /// <summary>
    /// Port, server is listening at
    /// </summary>
    ushort Port { get; }
}
