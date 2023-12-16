using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedTypeServiceDescriptor : IKeyedServiceDescriptor
{
    public Type ImplementationType { get; }
}
