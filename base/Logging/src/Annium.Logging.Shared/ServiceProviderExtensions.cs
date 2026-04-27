using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Logging.Shared.Internal;

namespace Annium.Logging.Shared;

/// <summary>
/// Extensions for IServiceProvider to configure logging routes
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Configures logging for a specific context type with route configuration
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="provider">The service provider</param>
    /// <param name="configure">Action to configure the log route</param>
    /// <returns>The service provider for chaining</returns>
    public static IServiceProvider UseLogging<TContext>(
        this IServiceProvider provider,
        Action<LogRoute<TContext>> configure
    )
        where TContext : class
    {
        var routes = new List<LogRoute<TContext>>();
        configure(new LogRoute<TContext>(provider, routes.Add));

        return provider.UseLoggingBase(routes);
    }

    /// <summary>
    /// Configures logging using the default log context with route configuration
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="configure">Action to configure the log route</param>
    /// <returns>The service provider for chaining</returns>
    public static IServiceProvider UseLogging(
        this IServiceProvider provider,
        Action<LogRoute<DefaultLogContext>> configure
    ) => provider.UseLogging<DefaultLogContext>(configure);

    /// <summary>
    /// Configures logging for a specific context type with route and service provider access
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="provider">The service provider</param>
    /// <param name="configure">Action to configure the log route with service provider access</param>
    /// <returns>The service provider for chaining</returns>
    public static IServiceProvider UseLogging<TContext>(
        this IServiceProvider provider,
        Action<LogRoute<TContext>, IServiceProvider> configure
    )
        where TContext : class => provider.UseLogging<TContext>(route => configure(route, provider));

    /// <summary>
    /// Configures logging using the default log context with route and service provider access
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="configure">Action to configure the log route with service provider access</param>
    /// <returns>The service provider for chaining</returns>
    public static IServiceProvider UseLogging(
        this IServiceProvider provider,
        Action<LogRoute<DefaultLogContext>, IServiceProvider> configure
    ) => provider.UseLogging<DefaultLogContext>((route, sp) => configure(route, sp));

    /// <summary>
    /// Internal method to configure logging base functionality with the provided routes
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="provider">The service provider</param>
    /// <param name="routes">The list of configured routes</param>
    /// <returns>The service provider for chaining</returns>
    private static IServiceProvider UseLoggingBase<TContext>(
        this IServiceProvider provider,
        List<LogRoute<TContext>> routes
    )
        where TContext : class
    {
        var schedulers = provider.Resolve<List<ILogScheduler<TContext>>>();

        foreach (var route in routes)
        {
            var handler = route.Handler.NotNull();
            var cfg = route.Configuration.NotNull();

            // Default selection: BufferingLogHandler-derived → background, everything else → immediate.
            var kind =
                route.SchedulerOverride
                ?? (
                    handler is BufferingLogHandler<TContext>
                        ? LogRouteSchedulerKind.Background
                        : LogRouteSchedulerKind.Immediate
                );

            schedulers.Add(
                kind == LogRouteSchedulerKind.Immediate
                    ? new ImmediateLogScheduler<TContext>(route.Filter, handler)
                    : new BackgroundLogScheduler<TContext>(route.Filter, handler, cfg)
            );
        }

        return provider;
    }
}
