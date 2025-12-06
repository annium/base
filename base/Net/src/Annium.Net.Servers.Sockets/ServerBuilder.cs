using System;
using System.Net;
using Annium.Core.DependencyInjection;
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
    /// <returns>A new server builder instance</returns>
    public static IServerBuilder New(IServiceProvider sp)
    {
        return new ServerBuilderInstance(sp, IPAddress.Loopback, 0);
    }

    /// <summary>
    /// Creates a new server builder instance for the specified port
    /// </summary>
    /// <param name="sp">Service provider for dependency injection</param>
    /// <param name="address">IP address to listen on</param>
    /// <returns>A new server builder instance</returns>
    public static IServerBuilder New(IServiceProvider sp, IPAddress address)
    {
        return new ServerBuilderInstance(sp, address, 0);
    }

    /// <summary>
    /// Creates a new server builder instance for the specified port
    /// </summary>
    /// <param name="sp">Service provider for dependency injection</param>
    /// <param name="port">Port number for the server to listen on</param>
    /// <returns>A new server builder instance</returns>
    public static IServerBuilder New(IServiceProvider sp, ushort port)
    {
        return new ServerBuilderInstance(sp, IPAddress.Loopback, port);
    }

    /// <summary>
    /// Creates a new server builder instance for the specified port
    /// </summary>
    /// <param name="sp">Service provider for dependency injection</param>
    /// <param name="address">IP address to listen on</param>
    /// <param name="port">Port number for the server to listen on</param>
    /// <returns>A new server builder instance</returns>
    public static IServerBuilder New(IServiceProvider sp, IPAddress address, ushort port)
    {
        return new ServerBuilderInstance(sp, address, port);
    }
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
    /// IP address to listen on
    /// </summary>
    private readonly IPAddress _address;

    /// <summary>
    /// Port number the server will listen on
    /// </summary>
    private readonly ushort _port;

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
    /// <param name="address">IP address to listen on</param>
    /// <param name="port">The port number the server will listen on (0 - for random free port).</param>
    public ServerBuilderInstance(IServiceProvider sp, IPAddress address, ushort port)
    {
        _sp = sp;
        _address = address;
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
    /// Builds and returns the configured server instance.
    /// </summary>
    /// <returns>The configured server instance. Null if failed to start http listener</returns>
    public IServer? Start()
    {
        if (_handler is null)
            throw new InvalidOperationException("Handler is not specified");

        var listener = TcpListenerResolver.Instance.Resolve(_address, _port);
        if (listener is null)
            return null;

        var uri = new Uri($"tcp://{listener.LocalEndpoint}");

        return new Server(listener, _handler, uri, _logger);
    }
}
