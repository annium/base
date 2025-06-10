using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Extensions.Workers.Internal;

namespace Annium.Extensions.Workers;

/// <summary>
/// Extension methods for configuring worker services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers worker management services in the dependency injection container
    /// </summary>
    /// <typeparam name="TData">The type of key used to identify workers</typeparam>
    /// <typeparam name="TWorker">The worker implementation type</typeparam>
    /// <param name="container">The service container to configure</param>
    /// <param name="lifetime">The lifetime for the worker manager service</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddWorkers<TData, TWorker>(
        this IServiceContainer container,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TData : IEquatable<TData>
        where TWorker : WorkerBase<TData>
    {
        container.Add<IWorkerManager<TData>, WorkerManager<TData>>().In(lifetime);
        container.Add<WorkerBase<TData>, TWorker>().Transient();

        return container;
    }
}
