using System;

namespace Annium.Core.DependencyInjection.Descriptors;

/// <summary>
/// Service descriptor that uses a concrete type implementation for the service
/// </summary>
public interface ITypeServiceDescriptor : IServiceDescriptor
{
    /// <summary>
    /// Type used to implement the service
    /// </summary>
    Type ImplementationType { get; }
}
