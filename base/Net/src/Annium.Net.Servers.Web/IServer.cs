using System;

namespace Annium.Net.Servers.Web;

/// <summary>
/// Defines a contract for a minimalistic web server that can be connected and awaited.
/// </summary>
public interface IServer : IAsyncDisposable
{
    /// <summary>
    /// Uri, that may be used to connect to server
    /// </summary>
    Uri Uri { get; }
}
