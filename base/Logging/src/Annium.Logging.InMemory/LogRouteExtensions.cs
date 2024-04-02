using Annium.Logging.InMemory;
using Annium.Logging.Shared;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class LogRouteExtensions
{
    public static LogRoute<TContext> UseInMemory<TContext>(this LogRoute<TContext> route)
        where TContext : class
    {
        route.UseInstance(new InMemoryLogHandler<TContext>(), new LogRouteConfiguration());

        return route;
    }

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
