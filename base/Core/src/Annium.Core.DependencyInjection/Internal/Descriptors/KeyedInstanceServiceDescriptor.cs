using System;
using Annium.Core.DependencyInjection.Descriptors;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for keyed instance-based services
/// </summary>
internal sealed record KeyedInstanceServiceDescriptor : IKeyedInstanceServiceDescriptor
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
    /// Gets the service instance
    /// </summary>
    public required object ImplementationInstance { get; init; }

    /// <summary>
    /// Gets the service lifetime
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }
}
