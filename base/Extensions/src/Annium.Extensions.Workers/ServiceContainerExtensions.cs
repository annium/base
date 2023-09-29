using System;
using Annium.Extensions.Workers;
using Annium.Extensions.Workers.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddWorkers<TData, TWorker>(
        this IServiceContainer container,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TData : IEquatable<TData>
        where TWorker : IWorker<TData>
    {
        container.Add<IWorkerManager<TData>, WorkerManager<TData>>().In(lifetime);
        container.Add<IWorker<TData>, TWorker>().Transient();

        return container;
    }
}