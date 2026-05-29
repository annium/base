using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for type-based services
/// </summary>
internal sealed record TypeServiceDescriptor : NonKeyedServiceDescriptorBase, ITypeServiceDescriptor
{
    /// <summary>
    /// Gets the implementation type
    /// </summary>
    public required Type ImplementationType { get; init; }
}
