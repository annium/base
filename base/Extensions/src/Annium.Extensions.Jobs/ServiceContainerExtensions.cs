using Annium.Extensions.Jobs;
using Annium.Extensions.Jobs.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for registering job scheduling services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers all job scheduling services including interval parser, resolver, and scheduler
    /// </summary>
    /// <param name="container">The service container to register services in</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddScheduler(this IServiceContainer container)
    {
        container.Add<IIntervalResolver, IntervalResolver>().Singleton();
        container.Add<IIntervalParser, IntervalParser>().Singleton();
        container.Add<IScheduler, Scheduler>().Singleton();

        return container;
    }
}
