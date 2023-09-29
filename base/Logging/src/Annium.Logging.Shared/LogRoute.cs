using System;

namespace Annium.Logging.Shared;

public class LogRoute<TContext>
    where TContext : class, ILogContext
{
    internal Func<LogMessage<TContext>, bool> Filter { get; private set; } = _ => true;
    internal object? Handler { get; private set; }
    internal LogRouteConfiguration? Configuration { get; private set; }
    private readonly IServiceProvider _sp;
    private readonly Action<LogRoute<TContext>> _registerRoute;

    internal LogRoute(
        IServiceProvider sp,
        Action<LogRoute<TContext>> registerRoute
    )
    {
        _sp = sp;
        _registerRoute = registerRoute;

        registerRoute(this);
    }

    public LogRoute<TContext> For(Func<LogMessage<TContext>, bool> filter) =>
        new(_sp, _registerRoute) { Filter = filter };

    public LogRoute<TContext> UseInstance<T>(T instance, LogRouteConfiguration configuration)
        where T : class, ILogHandler<TContext>
        => Use(instance, configuration);

    public LogRoute<TContext> UseFactory<T>(Func<IServiceProvider, T> factory, LogRouteConfiguration configuration)
        where T : class, ILogHandler<TContext>
        => Use(factory(_sp), configuration);

    public LogRoute<TContext> UseAsyncInstance<T>(T instance, LogRouteConfiguration configuration)
        where T : class, IAsyncLogHandler<TContext>
        => Use(instance, configuration);

    public LogRoute<TContext> UseAsyncFactory<T>(Func<IServiceProvider, T> factory, LogRouteConfiguration configuration)
        where T : class, IAsyncLogHandler<TContext>
        => Use(factory(_sp), configuration);

    private LogRoute<TContext> Use(object handler, LogRouteConfiguration configuration)
    {
        Handler = handler;
        Configuration = configuration;

        return this;
    }
}