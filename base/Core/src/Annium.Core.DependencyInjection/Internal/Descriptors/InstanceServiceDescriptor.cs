using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for instance-based services
/// </summary>
internal sealed record InstanceServiceDescriptor : IInstanceServiceDescriptor
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
    /// Gets the service instance
    /// </summary>
    public required object ImplementationInstance { get; init; }

    /// <summary>
    /// Gets the service lifetime
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }
}
