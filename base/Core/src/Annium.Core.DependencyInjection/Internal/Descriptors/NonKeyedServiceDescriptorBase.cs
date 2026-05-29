using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Shared base for non-keyed service descriptor records — encapsulates the trio of fields all
/// three flavours (type / factory / instance) carry identically: <see cref="ServiceType"/>,
/// <see cref="Key"/> (always <c>null</c> for non-keyed), and <see cref="Lifetime"/>.
/// Concrete records add their kind-specific field (implementation type / factory / instance).
/// </summary>
internal abstract record NonKeyedServiceDescriptorBase : IServiceDescriptor
{
    /// <summary>
    /// Gets the service type.
    /// </summary>
    public required Type ServiceType { get; init; }

    /// <summary>
    /// Gets the service key — always <see langword="null"/> for non-keyed descriptors.
    /// </summary>
    public object? Key => null;

    /// <summary>
    /// Gets the service lifetime.
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }
}
