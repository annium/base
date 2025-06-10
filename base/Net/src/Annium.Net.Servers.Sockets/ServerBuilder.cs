using System;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging;
using Annium.Net.Servers.Sockets.Internal;

namespace Annium.Net.Servers.Sockets;

/// <summary>
/// Provides factory methods for creating socket server builders
/// </summary>
public static class ServerBuilder
{
    /// <summary>
    /// Creates a new server builder instance for the specified port
    /// </summary>
    /// <param name="sp">Service provider for dependency injection</param>
    /// <param name="port">Port number for the server to listen on</param>
    /// <returns>A new server builder instance</returns>
    public static IServerBuilder New(IServiceProvider sp, int port)
    {
        return new ServerBuilderInstance(sp, port);
    }
}

/// <summary>
/// Defines a contract for building socket servers with fluent configuration
/// </summary>
public interface IServerBuilder
{
    /// <summary>
    /// Configures the server to use a handler of the specified type
    /// </summary>
    /// <typeparam name="THandler">The type of handler to use, must implement IHandler</typeparam>
    /// <returns>The server builder instance for method chaining</returns>
    IServerBuilder WithHandler<THandler>()
        where THandler : IHandler;

    /// <summary>
    /// Configures the server to use the specified handler instance
    /// </summary>
    /// <param name="handler">The handler instance to use for processing connections</param>
    /// <returns>The server builder instance for method chaining</returns>
    IServerBuilder WithHandler(IHandler handler);

    /// <summary>
    /// Builds and returns the configured server instance
    /// </summary>
    /// <returns>A configured server instance ready to run</returns>
    IServer Build();
}

/// <summary>
/// Internal implementation of the server builder interface
/// </summary>
file class ServerBuilderInstance : IServerBuilder
{
    /// <summary>
    /// Service provider for dependency injection
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// Port number the server will listen on
    /// </summary>
    private readonly int _port;

    /// <summary>
    /// Logger instance for the server builder
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Handler instance that will process incoming connections
    /// </summary>
    private IHandler? _handler;

    /// <summary>
    /// Initializes a new instance of the ServerBuilderInstance class
    /// </summary>
    /// <param name="sp">Service provider for dependency injection</param>
    /// <param name="port">Port number for the server to listen on</param>
    public ServerBuilderInstance(IServiceProvider sp, int port)
    {
        _sp = sp;
        _port = port;
        _logger = sp.Resolve<ILogger>();
    }

    /// <summary>
    /// Configures the server to use a handler of the specified type
    /// </summary>
    /// <typeparam name="THandler">The type of handler to use, must implement IHandler</typeparam>
    /// <returns>The server builder instance for method chaining</returns>
    public IServerBuilder WithHandler<THandler>()
        where THandler : IHandler
    {
        _handler = _sp.Resolve<THandler>();

        return this;
    }

    /// <summary>
    /// Configures the server to use the specified handler instance
    /// </summary>
    /// <param name="handler">The handler instance to use for processing connections</param>
    /// <returns>The server builder instance for method chaining</returns>
    public IServerBuilder WithHandler(IHandler handler)
    {
        _handler = handler;

        return this;
    }

    /// <summary>
    /// Builds and returns the configured server instance
    /// </summary>
    /// <returns>A configured server instance ready to run</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler has been configured</exception>
    public IServer Build()
    {
        if (_handler is null)
            throw new InvalidOperationException("Handler is not specified");

        return new Server(_port, _handler, _logger);
    }
}
