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
    /// Maximum attempts to locate an available port when selecting dynamically.
    /// </summary>
    private const int MaxDynamicAttempts = 100;

    /// <summary>
    /// Minimum port number considered during dynamic resolution.
    /// </summary>
    private const ushort MinPortNumber = 1000;

    /// <summary>
    /// Maximum port number considered during dynamic resolution.
    /// </summary>
    private const ushort MaxPortNumber = 65535;

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
        return port == 0 ? ResolveDynamic(address) : ResolveStatic(address, port);
    }

    /// <summary>
    /// Attempts to resolve a listener by randomly choosing a free port within the allowed range.
    /// </summary>
    /// <param name="address">IP address to bind the listener.</param>
    /// <returns>A started TCP listener if binding succeeds; otherwise null.</returns>
    private TcpListener? ResolveDynamic(IPAddress address)
    {
        var rnd = new Random();
        for (var i = 0; i < MaxDynamicAttempts; i++)
        {
            // get random port in range from 1000 (below are usually considered as system ports)
            var port = (ushort)rnd.Next(MinPortNumber, MaxPortNumber);
            var listener = ResolveStatic(address, port);
            if (listener is null)
                continue;

            return listener;
        }

        return null;
    }

    /// <summary>
    /// Attempts to create and start a listener on a specific port.
    /// </summary>
    /// <param name="address">IP address to bind the listener.</param>
    /// <param name="port">Port number to bind.</param>
    /// <returns>A started TCP listener if binding succeeds; otherwise null.</returns>
    private TcpListener? ResolveStatic(IPAddress address, ushort port)
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
