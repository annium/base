using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for keyed type-based services
/// </summary>
internal sealed record KeyedTypeServiceDescriptor : IKeyedTypeServiceDescriptor
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
    /// Gets the implementation type
    /// </summary>
    public required Type ImplementationType { get; init; }

    /// <summary>
    /// Gets the service lifetime
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }
}
