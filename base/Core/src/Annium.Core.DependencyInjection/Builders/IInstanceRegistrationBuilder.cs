using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IInstanceRegistrationBuilderBase
{
    IInstanceRegistrationBuilderBase AsSelf();
    IInstanceRegistrationBuilderBase As(Type serviceType);
    IInstanceRegistrationBuilderBase AsInterfaces();
    IInstanceRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull;
    IInstanceRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull;
    IInstanceRegistrationBuilderBase AsSelfFactory();
    IInstanceRegistrationBuilderBase AsFactory(Type serviceType);
    IInstanceRegistrationBuilderBase AsKeyedSelfFactory<TKey>(TKey key) where TKey : notnull;
    IInstanceRegistrationBuilderBase AsKeyedFactory<TKey>(Type serviceType, TKey key) where TKey : notnull;
    public IServiceContainer Singleton();
}