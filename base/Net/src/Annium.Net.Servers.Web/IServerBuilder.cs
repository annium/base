namespace Annium.Net.Servers.Web;

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
    IServer? Start();
}
