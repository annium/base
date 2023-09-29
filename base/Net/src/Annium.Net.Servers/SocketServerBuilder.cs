using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Servers.Internal;

namespace Annium.Net.Servers;

public static class SocketServerBuilder
{
    public static ISocketServerBuilder New(IServiceProvider sp, int port)
    {
        return new SocketServerBuilderInstance(sp, port);
    }
}

public interface ISocketServerBuilder
{
    ISocketServerBuilder WithHandler(Func<IServiceProvider, Socket, CancellationToken, Task> handler);
    ISocketServer Build();
}

file class SocketServerBuilderInstance : ISocketServerBuilder
{
    private readonly IServiceProvider _sp;
    private readonly int _port;
    private Func<IServiceProvider, Socket, CancellationToken, Task>? _handler;

    public SocketServerBuilderInstance(IServiceProvider sp, int port)
    {
        _sp = sp;
        _port = port;
    }

    public ISocketServerBuilder WithHandler(Func<IServiceProvider, Socket, CancellationToken, Task> handler)
    {
        _handler = handler;

        return this;
    }

    public ISocketServer Build()
    {
        if (_handler is null)
            throw new InvalidOperationException("Handler is not specified");

        return new SocketServer(_sp, _port, _handler);
    }
}