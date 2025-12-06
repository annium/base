namespace Annium.Net.Servers.Sockets;

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
    /// Builds and returns the configured server instance.
    /// </summary>
    /// <returns>The configured server instance.</returns>
    IServer? Start();
}
