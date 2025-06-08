using Annium.Logging.InMemory;
using Annium.Logging.Shared;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring in-memory log routing.
/// Provides fluent API for adding in-memory logging to log routes.
/// </summary>
public static class LogRouteExtensions
{
    /// <summary>
    /// Configures the log route to use in-memory storage with a new handler instance.
    /// </summary>
    /// <typeparam name="TContext">The type of the log context</typeparam>
    /// <param name="route">The log route to configure</param>
    /// <returns>The configured log route</returns>
    public static LogRoute<TContext> UseInMemory<TContext>(this LogRoute<TContext> route)
        where TContext : class
    {
        route.UseInstance(new InMemoryLogHandler<TContext>(), new LogRouteConfiguration());

        return route;
    }

    /// <summary>
    /// Configures the log route to use in-memory storage with a specific handler instance.
    /// </summary>
    /// <typeparam name="TContext">The type of the log context</typeparam>
    /// <param name="route">The log route to configure</param>
    /// <param name="handler">The specific in-memory log handler to use</param>
    /// <returns>The configured log route</returns>
    public static LogRoute<TContext> UseInMemory<TContext>(
        this LogRoute<TContext> route,
        InMemoryLogHandler<TContext> handler
    )
        where TContext : class
    {
        route.UseInstance(handler, new LogRouteConfiguration());

        return route;
    }
}
