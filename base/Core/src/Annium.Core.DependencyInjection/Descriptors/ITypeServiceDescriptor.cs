using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface ITypeServiceDescriptor : IServiceDescriptor
{
    public Type ImplementationType { get; }
}
