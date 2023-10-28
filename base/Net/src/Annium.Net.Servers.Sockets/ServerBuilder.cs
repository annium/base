using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
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
    IServerBuilder WithHandler<THandler>()
        where THandler : IHandler;

    IServerBuilder WithHandler(IHandler handler);

    IServer Build();
}

file class ServerBuilderInstance : IServerBuilder
{
    private readonly IServiceProvider _sp;
    private readonly int _port;
    private readonly ILogger _logger;
    private IHandler? _handler;

    public ServerBuilderInstance(IServiceProvider sp, int port)
    {
        _sp = sp;
        _port = port;
        _logger = sp.Resolve<ILogger>();
    }

    public IServerBuilder WithHandler<THandler>()
        where THandler : IHandler
    {
        _handler = _sp.Resolve<THandler>();

        return this;
    }

    public IServerBuilder WithHandler(IHandler handler)
    {
        _handler = handler;

        return this;
    }

    public IServer Build()
    {
        if (_handler is null)
            throw new InvalidOperationException("Handler is not specified");

        return new Server(_port, _handler, _logger);
    }
}
