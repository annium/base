using System;

namespace Annium.Net.Servers.Sockets;

/// <summary>
/// Defines a contract for a socket server that can be started and run asynchronously
/// </summary>
public interface IServer : IAsyncDisposable
{
    /// <summary>
    /// Uri, that may be used to connect to server
    /// </summary>
    Uri Uri { get; }
}
