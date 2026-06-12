namespace Annium.Logging.Shared;

/// <summary>
/// Fluent builder returned by <see cref="LogRoute{TContext}.Use(ILogHandler{TContext}, LogRouteConfiguration?)"/>
/// (and its factory overload) to expose scheduler-override hooks. Default scheduler selection picks
/// <see cref="LogRouteSchedulerKind.Background"/> when the handler derives from
/// <see cref="BufferingLogHandler{TContext}"/> and <see cref="LogRouteSchedulerKind.Immediate"/> otherwise;
/// the methods on this builder let the caller force the alternative.
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
public sealed class LogRouteBuilder<TContext>
    where TContext : class
{
    /// <summary>
    /// The route this builder configures.
    /// </summary>
    private readonly LogRoute<TContext> _route;

    internal LogRouteBuilder(LogRoute<TContext> route)
    {
        _route = route;
    }

    /// <summary>
    /// Forces the scheduler to <see cref="LogRouteSchedulerKind.Immediate"/> regardless of handler type.
    /// Rare — useful when debugging a buffering sink that you want to observe synchronously.
    /// </summary>
    /// <returns>The builder for chaining</returns>
    public LogRouteBuilder<TContext> WithImmediateScheduler()
    {
        _route.SchedulerOverride = LogRouteSchedulerKind.Immediate;
        return this;
    }

    /// <summary>
    /// Forces the scheduler to <see cref="LogRouteSchedulerKind.Background"/> regardless of handler type.
    /// Rare — useful when a non-buffering sink is slow enough that immediate dispatch would block the
    /// caller (e.g., a synchronous network handler wrapped without buffering).
    /// </summary>
    /// <returns>The builder for chaining</returns>
    public LogRouteBuilder<TContext> WithBackgroundScheduler()
    {
        _route.SchedulerOverride = LogRouteSchedulerKind.Background;
        return this;
    }
}
