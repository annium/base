using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IBulkRegistrationBuilderBase : IBulkRegistrationBuilderTarget
{
    IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate);
}

public interface IBulkRegistrationBuilderTarget : IBulkRegistrationBuilderLifetime
{
    IBulkRegistrationBuilderTarget AsSelf();
    IBulkRegistrationBuilderTarget As(Type serviceType);
    IBulkRegistrationBuilderTarget AsInterfaces();
    IBulkRegistrationBuilderTarget AsKeyedSelf<TKey>(Func<Type, TKey> getKey) where TKey : notnull;
    IBulkRegistrationBuilderTarget AsKeyed<TKey>(Type serviceType, Func<Type, TKey> getKey) where TKey : notnull;
    IBulkRegistrationBuilderTarget AsSelfFactory();
    IBulkRegistrationBuilderTarget AsFactory(Type serviceType);
    IBulkRegistrationBuilderTarget AsKeyedSelfFactory<TKey>(Func<Type, TKey> getKey) where TKey : notnull;
    IBulkRegistrationBuilderTarget AsKeyedFactory<TKey>(Type serviceType, Func<Type, TKey> getKey) where TKey : notnull;
}

public interface IBulkRegistrationBuilderLifetime
{
    IServiceContainer In(ServiceLifetime lifetime);
    IServiceContainer Scoped();
    IServiceContainer Singleton();
    IServiceContainer Transient();
}