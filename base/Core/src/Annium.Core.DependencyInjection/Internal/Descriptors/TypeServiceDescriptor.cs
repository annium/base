using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for type-based services
/// </summary>
internal sealed record TypeServiceDescriptor : ITypeServiceDescriptor
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
    /// Gets the implementation type
    /// </summary>
    public required Type ImplementationType { get; init; }

    /// <summary>
    /// Gets the service lifetime
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }
}
