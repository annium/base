using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedFactoryServiceDescriptor : IServiceDescriptor
{
    public Func<IServiceProvider, object, object> ImplementationFactory { get; }
}
