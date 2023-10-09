using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Servers.Sockets.Internal;

namespace Annium.Net.Servers.Sockets;

public static class ServerBuilder
{
    public static IServerBuilder New(IServiceProvider sp, int port)
    {
        return new ServerBuilderInstance(sp, port);
    }
}

public interface IServerBuilder
{
    IServerBuilder WithHandler(Func<IServiceProvider, Socket, CancellationToken, Task> handler);
    IServer Build();
}

file class ServerBuilderInstance : IServerBuilder
{
    private readonly IServiceProvider _sp;
    private readonly int _port;
    private Func<IServiceProvider, Socket, CancellationToken, Task>? _handler;

    public ServerBuilderInstance(IServiceProvider sp, int port)
    {
        _sp = sp;
        _port = port;
    }

    public IServerBuilder WithHandler(Func<IServiceProvider, Socket, CancellationToken, Task> handler)
    {
        _handler = handler;

        return this;
    }

    public IServer Build()
    {
        if (_handler is null)
            throw new InvalidOperationException("Handler is not specified");

        return new Server(_sp, _port, _handler);
    }
}