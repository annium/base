using System;

namespace Annium.Net.Servers.Web;

/// <summary>
/// Defines a contract for a minimalistic web server that can be connected and awaited.
/// </summary>
public interface IServer : IAsyncDisposable
{
    /// <summary>
    /// Whether server uses secure connection or not
    /// </summary>
    bool IsSecure { get; }

    /// <summary>
    /// Host, server is listening at
    /// </summary>
    string Host { get; }

    /// <summary>
    /// Port, server is listening at
    /// </summary>
    ushort Port { get; }
}
