using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IFactoryServiceDescriptor : IServiceDescriptor
{
    public Func<IServiceProvider, object> ImplementationFactory { get; }
}
