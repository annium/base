using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for keyed factory-based services
/// </summary>
internal sealed record KeyedFactoryServiceDescriptor : KeyedServiceDescriptorBase, IKeyedFactoryServiceDescriptor
{
    /// <summary>
    /// Gets the keyed factory function for creating service instances
    /// </summary>
    public required Func<IServiceProvider, object, object> ImplementationFactory { get; init; }
}
