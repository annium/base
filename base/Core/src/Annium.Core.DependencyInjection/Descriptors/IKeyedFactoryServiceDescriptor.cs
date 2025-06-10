using System;

namespace Annium.Core.DependencyInjection.Descriptors;

/// <summary>
/// Service descriptor that uses a keyed factory method to create service instances
/// </summary>
public interface IKeyedFactoryServiceDescriptor : IServiceDescriptor
{
    /// <summary>
    /// Keyed factory method used to create service instances
    /// </summary>
    Func<IServiceProvider, object, object> ImplementationFactory { get; }
}
