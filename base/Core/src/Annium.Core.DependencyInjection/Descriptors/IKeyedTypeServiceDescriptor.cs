using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Service descriptor that uses a concrete type implementation for a keyed service
/// </summary>
public interface IKeyedTypeServiceDescriptor : IServiceDescriptor
{
    /// <summary>
    /// Type used to implement the keyed service
    /// </summary>
    Type ImplementationType { get; }
}
