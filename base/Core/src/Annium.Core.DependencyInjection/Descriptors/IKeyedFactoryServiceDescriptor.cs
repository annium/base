using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedFactoryServiceDescriptor : IServiceDescriptor
{
    Func<IServiceProvider, object, object> ImplementationFactory { get; }
}
