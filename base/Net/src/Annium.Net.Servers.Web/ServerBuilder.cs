using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Servers.Web.Internal;

namespace Annium.Net.Servers.Web;

public static class ServerBuilder
{
    public static IServerBuilder New(IServiceProvider sp, int port)
    {
        return new ServerBuilderInstance(sp, port);
    }
}

public interface IServerBuilder
{
    IServerBuilder WithHttpHandler<THandler>()
        where THandler : IHttpHandler;

    IServerBuilder WithHttpHandler(IHttpHandler handler);

    IServerBuilder WithWebSocketHandler<THandler>()
        where THandler : IWebSocketHandler;

    IServerBuilder WithWebSocketHandler(IWebSocketHandler handler);

    IServer Build();
}

file class ServerBuilderInstance : IServerBuilder
{
    private readonly IServiceProvider _sp;
    private readonly int _port;
    private IHttpHandler? _httpHandler;
    private IWebSocketHandler? _webSocketHandler;
    private readonly ILogger _logger;

    public ServerBuilderInstance(IServiceProvider sp, int port)
    {
        _sp = sp;
        _port = port;
        _logger = sp.Resolve<ILogger>();
    }

    public IServerBuilder WithHttpHandler<THandler>()
        where THandler : IHttpHandler
    {
        _httpHandler = _sp.Resolve<THandler>();

        return this;
    }

    public IServerBuilder WithHttpHandler(IHttpHandler handler)
    {
        _httpHandler = handler;

        return this;
    }

    public IServerBuilder WithWebSocketHandler<THandler>()
        where THandler : IWebSocketHandler
    {
        _webSocketHandler = _sp.Resolve<THandler>();

        return this;
    }

    public IServerBuilder WithWebSocketHandler(IWebSocketHandler handler)
    {
        _webSocketHandler = handler;

        return this;
    }

    public IServer Build()
    {
        return new Server(_port, _httpHandler, _webSocketHandler, _logger);
    }
}