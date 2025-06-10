using System;
using Annium.Core.DependencyInjection.Descriptors;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for keyed factory-based services
/// </summary>
internal sealed record KeyedFactoryServiceDescriptor : IKeyedFactoryServiceDescriptor
{
    /// <summary>
    /// Gets the service type
    /// </summary>
    public required Type ServiceType { get; init; }

    /// <summary>
    /// Gets the service key
    /// </summary>
    public required object Key { get; init; }

    /// <summary>
    /// Gets the keyed factory function for creating service instances
    /// </summary>
    public required Func<IServiceProvider, object, object> ImplementationFactory { get; init; }

    /// <summary>
    /// Gets the service lifetime
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }
}
