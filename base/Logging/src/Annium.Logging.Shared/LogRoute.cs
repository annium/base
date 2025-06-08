using System;
using OneOf;

namespace Annium.Logging.Shared;

/// <summary>
/// Represents a logging route configuration for a specific context type
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
public class LogRoute<TContext>
    where TContext : class
{
    /// <summary>
    /// Default filter that allows all log messages
    /// </summary>
    private static readonly Func<LogMessage<TContext>, bool> _logAll = _ => true;

    /// <summary>
    /// Gets the filter function for this route
    /// </summary>
    public Func<LogMessage<TContext>, bool> Filter { get; private set; } = _logAll;

    /// <summary>
    /// Gets the handler for this route
    /// </summary>
    internal OneOf<ILogHandler<TContext>, IAsyncLogHandler<TContext>>? Handler { get; private set; }

    /// <summary>
    /// Gets the configuration for this route
    /// </summary>
    internal LogRouteConfiguration? Configuration { get; private set; }

    /// <summary>
    /// The service provider for dependency resolution
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// Action to register this route
    /// </summary>
    private readonly Action<LogRoute<TContext>> _registerRoute;

    internal LogRoute(IServiceProvider sp, Action<LogRoute<TContext>> registerRoute)
    {
        _sp = sp;
        _registerRoute = registerRoute;

        registerRoute(this);
    }

    /// <summary>
    /// Creates a route that accepts all log messages
    /// </summary>
    /// <returns>A new log route configured to accept all messages</returns>
    public LogRoute<TContext> ForAll() => new(_sp, _registerRoute) { Filter = _logAll };

    /// <summary>
    /// Creates a route with a custom filter
    /// </summary>
    /// <param name="filter">The filter function to apply</param>
    /// <returns>A new log route with the specified filter</returns>
    public LogRoute<TContext> For(Func<LogMessage<TContext>, bool> filter) =>
        new(_sp, _registerRoute) { Filter = filter };

    /// <summary>
    /// Configures the route to use a specific log handler instance
    /// </summary>
    /// <typeparam name="T">The type of the log handler</typeparam>
    /// <param name="instance">The handler instance to use</param>
    /// <param name="configuration">The route configuration</param>
    /// <returns>The configured log route</returns>
    public LogRoute<TContext> UseInstance<T>(T instance, LogRouteConfiguration configuration)
        where T : class, ILogHandler<TContext> => Use(instance, configuration);

    /// <summary>
    /// Configures the route to use a log handler created by a factory function
    /// </summary>
    /// <typeparam name="T">The type of the log handler</typeparam>
    /// <param name="factory">The factory function to create the handler</param>
    /// <param name="configuration">The route configuration</param>
    /// <returns>The configured log route</returns>
    public LogRoute<TContext> UseFactory<T>(Func<IServiceProvider, T> factory, LogRouteConfiguration configuration)
        where T : class, ILogHandler<TContext> => Use(factory(_sp), configuration);

    /// <summary>
    /// Configures the route to use a specific async log handler instance
    /// </summary>
    /// <typeparam name="T">The type of the async log handler</typeparam>
    /// <param name="instance">The handler instance to use</param>
    /// <param name="configuration">The route configuration</param>
    /// <returns>The configured log route</returns>
    public LogRoute<TContext> UseAsyncInstance<T>(T instance, LogRouteConfiguration configuration)
        where T : class, IAsyncLogHandler<TContext> => Use(instance, configuration);

    /// <summary>
    /// Configures the route to use an async log handler created by a factory function
    /// </summary>
    /// <typeparam name="T">The type of the async log handler</typeparam>
    /// <param name="factory">The factory function to create the handler</param>
    /// <param name="configuration">The route configuration</param>
    /// <returns>The configured log route</returns>
    public LogRoute<TContext> UseAsyncFactory<T>(Func<IServiceProvider, T> factory, LogRouteConfiguration configuration)
        where T : class, IAsyncLogHandler<TContext> => Use(factory(_sp), configuration);

    /// <summary>
    /// Internal method to configure the route with a handler and configuration
    /// </summary>
    /// <param name="handler">The handler to use</param>
    /// <param name="configuration">The route configuration</param>
    /// <returns>The configured log route</returns>
    private LogRoute<TContext> Use(
        OneOf<ILogHandler<TContext>, IAsyncLogHandler<TContext>> handler,
        LogRouteConfiguration configuration
    )
    {
        Handler = handler;
        Configuration = configuration;

        return this;
    }
}
