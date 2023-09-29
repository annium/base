using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IServiceContainer : IEnumerable<IServiceDescriptor>
{
    int Count { get; }
    IServiceCollection Collection { get; }
    event Action<IServiceProvider> OnBuild;
    IServiceContainer Add(IServiceDescriptor descriptor);
    IBulkRegistrationBuilderBase Add(IEnumerable<Type> types);
    IFactoryRegistrationBuilderBase Add(Type type, Func<IServiceProvider, object> factory);
    IInstanceRegistrationBuilderBase Add<T>(T instance) where T : class;
    ISingleRegistrationBuilderBase Add(Type type);
    IFactoryRegistrationBuilderBase Add<T>(Func<IServiceProvider, T> factory) where T : class;
    ISingleRegistrationBuilderBase Add<TService, TImplementation>() where TImplementation : TService;
    ISingleRegistrationBuilderBase Add<TImplementationType>();
    IServiceContainer Clone();
    bool Contains(IServiceDescriptor descriptor);
    ServiceProvider BuildServiceProvider();
}