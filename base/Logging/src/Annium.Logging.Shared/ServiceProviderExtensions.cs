using System;
using System.Collections.Generic;
using Annium.Logging.Shared;
using Annium.Logging.Shared.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static IServiceProvider UseLogging<TContext>(
        this IServiceProvider provider,
        Action<LogRoute<TContext>> configure
    )
        where TContext : class, ILogContext
    {
        var routes = new List<LogRoute<TContext>>();
        configure(new LogRoute<TContext>(provider, routes.Add));

        return provider.UseLoggingBase(routes);
    }

    public static IServiceProvider UseLogging(
        this IServiceProvider provider,
        Action<LogRoute<DefaultLogContext>> configure
    )
    {
        var routes = new List<LogRoute<DefaultLogContext>>();
        configure(new LogRoute<DefaultLogContext>(provider, routes.Add));

        return provider.UseLoggingBase(routes);
    }

    public static IServiceProvider UseLogging<TContext>(
        this IServiceProvider provider,
        Action<LogRoute<TContext>, IServiceProvider> configure
    )
        where TContext : class, ILogContext
    {
        var routes = new List<LogRoute<TContext>>();
        configure(new LogRoute<TContext>(provider, routes.Add), provider);

        return provider.UseLoggingBase(routes);
    }

    public static IServiceProvider UseLogging(
        this IServiceProvider provider,
        Action<LogRoute<DefaultLogContext>, IServiceProvider> configure
    )
    {
        var routes = new List<LogRoute<DefaultLogContext>>();
        configure(new LogRoute<DefaultLogContext>(provider, routes.Add), provider);

        return provider.UseLoggingBase(routes);
    }

    private static IServiceProvider UseLoggingBase<TContext>(
        this IServiceProvider provider,
        List<LogRoute<TContext>> routes
    )
        where TContext : class, ILogContext
    {
        var schedulers = provider.Resolve<List<ILogScheduler<TContext>>>();

        foreach (var route in routes)
        {
            if (route.Handler is null)
                continue;

            var handler = route.Handler;
            var cfg = route.Configuration!;

            switch (handler)
            {
                case ILogHandler<TContext> syncHandler:
                    schedulers.Add(new ImmediateLogScheduler<TContext>(route.Filter, syncHandler));
                    break;
                case IAsyncLogHandler<TContext> asyncHandler:
                    schedulers.Add(new BackgroundLogScheduler<TContext>(route.Filter, asyncHandler, cfg));
                    break;
                default:
                    throw new Exception($"Unsupported log handler: {handler.GetType().FriendlyName()}");
            }
        }

        return provider;
    }
}