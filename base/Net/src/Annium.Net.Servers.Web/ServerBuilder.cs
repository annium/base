using System;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Servers.Web.Internal;

namespace Annium.Net.Servers.Web;

/// <summary>
/// Factory class for creating server builder instances.
/// </summary>
public static class ServerBuilder
{
    /// <summary>
    /// Creates a new server builder instance configured for the specified port.
    /// </summary>
    /// <param name="sp">The service provider for dependency injection.</param>
    /// <param name="isSecure">Whether to use https:// or http:// scheme in listener</param>
    /// <param name="host">host name to listen on</param>
    /// <param name="port">The port number the server will listen on (0 - for random free port).</param>
    /// <returns>A new server builder instance.</returns>
    public static IServerBuilder New(IServiceProvider sp, bool isSecure = false, string host = "*", ushort port = 0)
    {
        return new ServerBuilderInstance(sp, isSecure, host, port);
    }
}

/// <summary>
/// Internal implementation of the server builder interface.
/// </summary>
file class ServerBuilderInstance : IServerBuilder
{
    /// <summary>
    /// The service provider for dependency injection.
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// Whether to use https:// or http:// scheme in listener.
    /// </summary>
    private readonly bool _isSecure;

    /// <summary>
    /// IP Address to listen on.
    /// </summary>
    private readonly string _host;

    /// <summary>
    /// The port number the server will listen on (0 - for random free port).
    /// </summary>
    private readonly ushort _port;

    /// <summary>
    /// The configured HTTP handler instance.
    /// </summary>
    private IHttpHandler? _httpHandler;

    /// <summary>
    /// The configured WebSocket handler instance.
    /// </summary>
    private IWebSocketHandler? _webSocketHandler;

    /// <summary>
    /// The logger instance for the server builder.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the ServerBuilderInstance class.
    /// </summary>
    /// <param name="sp">The service provider for dependency injection.</param>
    /// <param name="isSecure">Whether to use https:// or http:// scheme in listener</param>
    /// <param name="host">host name to listen on</param>
    /// <param name="port">The port number the server will listen on (0 - for random free port).</param>
    public ServerBuilderInstance(IServiceProvider sp, bool isSecure, string host, ushort port)
    {
        _sp = sp;
        _isSecure = isSecure;
        _host = host;
        _port = port;
        _logger = sp.Resolve<ILogger>();
    }

    /// <summary>
    /// Configures the server to use a specific HTTP handler type resolved from the service provider.
    /// </summary>
    /// <typeparam name="THandler">The type of HTTP handler to use.</typeparam>
    /// <returns>The server builder instance for method chaining.</returns>
    public IServerBuilder WithHttpHandler<THandler>()
        where THandler : IHttpHandler
    {
        _httpHandler = _sp.Resolve<THandler>();

        return this;
    }

    /// <summary>
    /// Configures the server to use a specific HTTP handler instance.
    /// </summary>
    /// <param name="handler">The HTTP handler instance to use.</param>
    /// <returns>The server builder instance for method chaining.</returns>
    public IServerBuilder WithHttpHandler(IHttpHandler handler)
    {
        _httpHandler = handler;

        return this;
    }

    /// <summary>
    /// Configures the server to use a specific WebSocket handler type resolved from the service provider.
    /// </summary>
    /// <typeparam name="THandler">The type of WebSocket handler to use.</typeparam>
    /// <returns>The server builder instance for method chaining.</returns>
    public IServerBuilder WithWebSocketHandler<THandler>()
        where THandler : IWebSocketHandler
    {
        _webSocketHandler = _sp.Resolve<THandler>();

        return this;
    }

    /// <summary>
    /// Configures the server to use a specific WebSocket handler instance.
    /// </summary>
    /// <param name="handler">The WebSocket handler instance to use.</param>
    /// <returns>The server builder instance for method chaining.</returns>
    public IServerBuilder WithWebSocketHandler(IWebSocketHandler handler)
    {
        _webSocketHandler = handler;

        return this;
    }

    /// <summary>
    /// Builds and returns the configured server instance.
    /// </summary>
    /// <returns>The configured server instance. Null if failed to start http listener</returns>
    public IServer? Start()
    {
        var listener = HttpListenerResolver.Instance.Resolve(_isSecure, _host, _port);
        if (listener is null)
            return null;

        var prefix = listener.Prefixes.First();
        var uri = new Uri(_host is "*" or "+" ? prefix.Replace(_host, "127.0.0.1") : prefix);

        return new Server(listener, _httpHandler, _webSocketHandler, uri, _logger);
    }
}
