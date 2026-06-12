using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for keyed type-based services
/// </summary>
internal sealed record KeyedTypeServiceDescriptor : KeyedServiceDescriptorBase, IKeyedTypeServiceDescriptor
{
    /// <summary>
    /// Gets the implementation type
    /// </summary>
    public required Type ImplementationType { get; init; }
}
