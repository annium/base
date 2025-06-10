using System;
using Annium.Core.DependencyInjection.Descriptors;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for factory-based services
/// </summary>
internal sealed record FactoryServiceDescriptor : IFactoryServiceDescriptor
{
    /// <summary>
    /// Gets the service type
    /// </summary>
    public required Type ServiceType { get; init; }

    /// <summary>
    /// Gets the service key (always null for non-keyed services)
    /// </summary>
    public object? Key => null;

    /// <summary>
    /// Gets the factory function for creating service instances
    /// </summary>
    public required Func<IServiceProvider, object> ImplementationFactory { get; init; }

    /// <summary>
    /// Gets the service lifetime
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }
}
