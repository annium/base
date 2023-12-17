using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedFactoryServiceDescriptor : IKeyedServiceDescriptor
{
    public Func<IServiceProvider, object, object> ImplementationFactory { get; }
}
