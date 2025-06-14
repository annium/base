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
    )
    {
        var routes = new List<LogRoute<DefaultLogContext>>();
        configure(new LogRoute<DefaultLogContext>(provider, routes.Add));

        return provider.UseLoggingBase(routes);
    }

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
        where TContext : class
    {
        var routes = new List<LogRoute<TContext>>();
        configure(new LogRoute<TContext>(provider, routes.Add), provider);

        return provider.UseLoggingBase(routes);
    }

    /// <summary>
    /// Configures logging using the default log context with route and service provider access
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="configure">Action to configure the log route with service provider access</param>
    /// <returns>The service provider for chaining</returns>
    public static IServiceProvider UseLogging(
        this IServiceProvider provider,
        Action<LogRoute<DefaultLogContext>, IServiceProvider> configure
    )
    {
        var routes = new List<LogRoute<DefaultLogContext>>();
        configure(new LogRoute<DefaultLogContext>(provider, routes.Add), provider);

        return provider.UseLoggingBase(routes);
    }

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
            if (route.Handler is null)
                continue;

            var handler = route.Handler.NotNull();
            var cfg = route.Configuration.NotNull();

            handler.Switch(
                x =>
                {
                    schedulers.Add(new ImmediateLogScheduler<TContext>(route.Filter, x));
                },
                x =>
                {
                    schedulers.Add(new BackgroundLogScheduler<TContext>(route.Filter, x, cfg));
                }
            );
        }

        return provider;
    }
}
