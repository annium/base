using System;

namespace Annium.Logging.Shared;

/// <summary>
/// Represents a logging route configuration for a specific context type.
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
    internal ILogHandler<TContext>? Handler { get; private set; }

    /// <summary>
    /// Gets the configuration for this route
    /// </summary>
    internal LogRouteConfiguration? Configuration { get; private set; }

    /// <summary>
    /// Optional override for scheduler selection. When <c>null</c>, the route picks
    /// <see cref="LogRouteSchedulerKind.Background"/> for handlers derived from
    /// <see cref="BufferingLogHandler{TContext}"/> and <see cref="LogRouteSchedulerKind.Immediate"/> otherwise.
    /// </summary>
    internal LogRouteSchedulerKind? SchedulerOverride { get; set; }

    /// <summary>
    /// Tracks whether <see cref="Use(ILogHandler{TContext}, LogRouteConfiguration?)"/>
    /// or its factory overload has already been called on this instance. Each
    /// <see cref="LogRoute{TContext}"/> may be configured (and registered) at most once.
    /// </summary>
    private bool _isConfigured;

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
    /// Configures the route to use the given log handler instance and registers it
    /// into the parent provider's routes list. Must be called exactly once per route
    /// instance — a second call throws <see cref="InvalidOperationException"/>.
    /// Scheduler is auto-picked from the handler type unless overridden via the returned builder.
    /// </summary>
    /// <param name="handler">The handler instance to use</param>
    /// <param name="configuration">Optional route configuration; defaults to a new <see cref="LogRouteConfiguration"/></param>
    /// <returns>A builder exposing scheduler-override fluent hooks</returns>
    /// <exception cref="InvalidOperationException">Thrown when <c>Use</c> has already been called on this instance</exception>
    public LogRouteBuilder<TContext> Use(ILogHandler<TContext> handler, LogRouteConfiguration? configuration = null)
    {
        if (_isConfigured)
            throw new InvalidOperationException("LogRoute is already configured");

        Handler = handler;
        Configuration = configuration ?? new LogRouteConfiguration();
        _isConfigured = true;
        _registerRoute(this);
        return new LogRouteBuilder<TContext>(this);
    }

    /// <summary>
    /// Configures the route to use a log handler created by a factory function and registers
    /// it into the parent provider's routes list. Must be called exactly once per route
    /// instance — a second call throws <see cref="InvalidOperationException"/>.
    /// Scheduler is auto-picked from the handler type unless overridden via the returned builder.
    /// </summary>
    /// <param name="factory">The factory function to create the handler</param>
    /// <param name="configuration">Optional route configuration; defaults to a new <see cref="LogRouteConfiguration"/></param>
    /// <returns>A builder exposing scheduler-override fluent hooks</returns>
    /// <exception cref="InvalidOperationException">Thrown when <c>Use</c> has already been called on this instance</exception>
    public LogRouteBuilder<TContext> Use(
        Func<IServiceProvider, ILogHandler<TContext>> factory,
        LogRouteConfiguration? configuration = null
    )
    {
        if (_isConfigured)
            throw new InvalidOperationException("LogRoute is already configured");

        Handler = factory(_sp);
        Configuration = configuration ?? new LogRouteConfiguration();
        _isConfigured = true;
        _registerRoute(this);
        return new LogRouteBuilder<TContext>(this);
    }
}
