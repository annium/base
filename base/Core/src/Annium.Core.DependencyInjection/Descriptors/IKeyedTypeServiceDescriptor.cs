using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedTypeServiceDescriptor : IServiceDescriptor
{
    public Type ImplementationType { get; }
}
