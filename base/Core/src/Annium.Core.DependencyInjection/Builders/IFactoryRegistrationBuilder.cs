using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IFactoryRegistrationBuilderBase : IFactoryRegistrationBuilderLifetime
{
    IFactoryRegistrationBuilderBase AsSelf();
    IFactoryRegistrationBuilderBase As(Type serviceType);
    IFactoryRegistrationBuilderBase AsInterfaces();
}

public interface IFactoryRegistrationBuilderLifetime
{
    IServiceContainer In(ServiceLifetime lifetime);
    IServiceContainer Scoped();
    IServiceContainer Singleton();
    IServiceContainer Transient();
}
