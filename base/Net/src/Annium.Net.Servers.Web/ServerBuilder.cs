using System;
using Annium.Core.DependencyInjection.Extensions;
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
    /// <param name="port">The port number the server will listen on.</param>
    /// <returns>A new server builder instance.</returns>
    public static IServerBuilder New(IServiceProvider sp, int port)
    {
        return new ServerBuilderInstance(sp, port);
    }
}

/// <summary>
/// Defines a contract for building web servers with configurable HTTP and WebSocket handlers.
/// </summary>
public interface IServerBuilder
{
    /// <summary>
    /// Configures the server to use a specific HTTP handler type resolved from the service provider.
    /// </summary>
    /// <typeparam name="THandler">The type of HTTP handler to use.</typeparam>
    /// <returns>The server builder instance for method chaining.</returns>
    IServerBuilder WithHttpHandler<THandler>()
        where THandler : IHttpHandler;

    /// <summary>
    /// Configures the server to use a specific HTTP handler instance.
    /// </summary>
    /// <param name="handler">The HTTP handler instance to use.</param>
    /// <returns>The server builder instance for method chaining.</returns>
    IServerBuilder WithHttpHandler(IHttpHandler handler);

    /// <summary>
    /// Configures the server to use a specific WebSocket handler type resolved from the service provider.
    /// </summary>
    /// <typeparam name="THandler">The type of WebSocket handler to use.</typeparam>
    /// <returns>The server builder instance for method chaining.</returns>
    IServerBuilder WithWebSocketHandler<THandler>()
        where THandler : IWebSocketHandler;

    /// <summary>
    /// Configures the server to use a specific WebSocket handler instance.
    /// </summary>
    /// <param name="handler">The WebSocket handler instance to use.</param>
    /// <returns>The server builder instance for method chaining.</returns>
    IServerBuilder WithWebSocketHandler(IWebSocketHandler handler);

    /// <summary>
    /// Builds and returns the configured server instance.
    /// </summary>
    /// <returns>The configured server instance.</returns>
    IServer Build();
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
    /// The port number the server will listen on.
    /// </summary>
    private readonly int _port;

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
    /// <param name="port">The port number the server will listen on.</param>
    public ServerBuilderInstance(IServiceProvider sp, int port)
    {
        _sp = sp;
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
    /// <returns>The configured server instance.</returns>
    public IServer Build()
    {
        return new Server(_port, _httpHandler, _webSocketHandler, _logger);
    }
}
