using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IFactoryRegistrationBuilderBase : IFactoryRegistrationBuilderLifetime
{
    IFactoryRegistrationBuilderBase AsSelf();
    IFactoryRegistrationBuilderBase As(Type serviceType);
    IFactoryRegistrationBuilderBase AsInterfaces();
    IFactoryRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull;
    IFactoryRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull;
}

public interface IFactoryRegistrationBuilderLifetime
{
    IServiceContainer In(ServiceLifetime lifetime);
    IServiceContainer Scoped();
    IServiceContainer Singleton();
    IServiceContainer Transient();
}