using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Shared base for keyed service descriptor records — encapsulates the trio of fields all three
/// flavours (type / factory / instance) carry identically: <see cref="ServiceType"/>,
/// <see cref="Key"/> (always non-null for keyed descriptors), and <see cref="Lifetime"/>.
/// Concrete records add their kind-specific field (implementation type / factory / instance).
/// Mirrors <see cref="NonKeyedServiceDescriptorBase"/>.
/// </summary>
internal abstract record KeyedServiceDescriptorBase : IServiceDescriptor
{
    /// <summary>
    /// Gets the service type.
    /// </summary>
    public required Type ServiceType { get; init; }

    /// <summary>
    /// Gets the service key — always non-null for keyed descriptors.
    /// </summary>
    public required object Key { get; init; }

    /// <summary>
    /// Gets the service lifetime.
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }
}
