using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IServiceDescriptor
{
    public ServiceLifetime Lifetime { get; }

    public Type ServiceType { get; }
}

public interface ITypeServiceDescriptor : IServiceDescriptor
{
    public Type ImplementationType { get; }
}

public interface IFactoryServiceDescriptor : IServiceDescriptor
{
    public Func<IServiceProvider, object> ImplementationFactory { get; }
}

public interface IInstanceServiceDescriptor : IServiceDescriptor
{
    public object ImplementationInstance { get; }
}