using System;
using System.Net;
using System.Net.Sockets;

namespace Annium.Net.Servers.Sockets.Internal;

internal class TcpListenerResolver
{
    private const int MaxDynamicAttempts = 100;
    private const ushort MinPortNumber = 1000;
    private const ushort MaxPortNumber = 65535;

    public static readonly TcpListenerResolver Instance = new();

    private TcpListenerResolver() { }

    public TcpListener? Resolve(IPAddress address, ushort port)
    {
        return port == 0 ? ResolveDynamic(address) : ResolveStatic(address, port);
    }

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
