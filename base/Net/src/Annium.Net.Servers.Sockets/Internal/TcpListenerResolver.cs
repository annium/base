using System;
using System.Net;
using System.Net.Sockets;

namespace Annium.Net.Servers.Sockets.Internal;

/// <summary>
/// Resolves <see cref="TcpListener" /> instances for specified addresses and ports.
/// </summary>
internal class TcpListenerResolver
{
    /// <summary>
    /// Singleton instance of the resolver.
    /// </summary>
    public static readonly TcpListenerResolver Instance = new();

    private TcpListenerResolver() { }

    /// <summary>
    /// Resolves a TCP listener for the specified address and port, falling back to dynamic selection when port is zero.
    /// </summary>
    /// <param name="address">IP address to bind the listener.</param>
    /// <param name="port">Port to bind, or zero to choose dynamically.</param>
    /// <returns>A started TCP listener if binding succeeds; otherwise null.</returns>
    public TcpListener? Resolve(IPAddress address, ushort port)
    {
        try
        {
            var listener = new TcpListener(address, port);
            listener.Server.NoDelay = true;
            listener.Start();

            return listener;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
