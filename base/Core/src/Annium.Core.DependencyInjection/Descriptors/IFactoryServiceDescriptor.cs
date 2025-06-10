using System;

namespace Annium.Core.DependencyInjection.Descriptors;

/// <summary>
/// Service descriptor that uses a factory method to create service instances
/// </summary>
public interface IFactoryServiceDescriptor : IServiceDescriptor
{
    /// <summary>
    /// Factory method used to create service instances
    /// </summary>
    Func<IServiceProvider, object> ImplementationFactory { get; }
}
