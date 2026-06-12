using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for factory-based services
/// </summary>
internal sealed record FactoryServiceDescriptor : NonKeyedServiceDescriptorBase, IFactoryServiceDescriptor
{
    /// <summary>
    /// Gets the factory function for creating service instances
    /// </summary>
    public required Func<IServiceProvider, object> ImplementationFactory { get; init; }
}
