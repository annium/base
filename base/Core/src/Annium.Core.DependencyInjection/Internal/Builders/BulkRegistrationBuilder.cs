using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

internal class BulkRegistrationBuilder : IBulkRegistrationBuilderBase
{
    private readonly List<Type> _types;
    private readonly IServiceContainer _container;
    private readonly Registrar _registrar;
    private readonly RegistrationsCollection _registrations = new();

    public BulkRegistrationBuilder(
        IServiceContainer container,
        IEnumerable<Type> types,
        Registrar registrar
    )
    {
        _types = types.ToList();
        _container = container;
        _registrar = registrar;
    }

    public IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate)
    {
        _types.RemoveAll(x => !predicate(x));

        return this;
    }

    public IBulkRegistrationBuilderTarget AsSelf() =>
        WithRegistration(type => new TypeRegistration(type, type));

    public IBulkRegistrationBuilderTarget As(Type serviceType) =>
        WithRegistration(type => new TypeRegistration(serviceType, type));

    public IBulkRegistrationBuilderTarget AsInterfaces() =>
        WithRegistrations(type => type.GetInterfaces().Select(x => new TypeRegistration(x, type)));

    public IBulkRegistrationBuilderTarget AsKeyedSelf<TKey>(Func<Type, TKey> getKey) where TKey : notnull =>
        WithRegistration(type => new TypeKeyedRegistration(type, type, typeof(TKey), getKey(type)));

    public IBulkRegistrationBuilderTarget AsKeyed<TKey>(Type serviceType, Func<Type, TKey> getKey) where TKey : notnull =>
        WithRegistration(type => new TypeKeyedRegistration(serviceType, type, typeof(TKey), getKey(type)));

    public IBulkRegistrationBuilderTarget AsSelfFactory() =>
        WithRegistration(type => new TypeFactoryRegistration(type, type));

    public IBulkRegistrationBuilderTarget AsFactory(Type serviceType) =>
        WithRegistration(type => new TypeFactoryRegistration(serviceType, type));

    public IBulkRegistrationBuilderTarget AsKeyedSelfFactory<TKey>(Func<Type, TKey> getKey) where TKey : notnull =>
        WithRegistration(type => new TypeKeyedFactoryRegistration(type, type, typeof(TKey), getKey(type)));

    public IBulkRegistrationBuilderTarget AsKeyedFactory<TKey>(Type serviceType, Func<Type, TKey> getKey) where TKey : notnull =>
        WithRegistration(type => new TypeKeyedFactoryRegistration(serviceType, type, typeof(TKey), getKey(type)));

    public IServiceContainer In(ServiceLifetime lifetime)
    {
        _registrations.AddRange(_types.Select(x => new TypeRegistration(x, x)));
        _registrar.Register(_registrations, lifetime);

        return _container;
    }

    public IServiceContainer Scoped() => In(ServiceLifetime.Scoped);
    public IServiceContainer Singleton() => In(ServiceLifetime.Singleton);
    public IServiceContainer Transient() => In(ServiceLifetime.Transient);

    private IBulkRegistrationBuilderTarget WithRegistrations(Func<Type, IEnumerable<IRegistration>> createRegistrations)
    {
        _registrations.Init();
        _registrations.AddRange(_types.SelectMany(createRegistrations));

        return this;
    }

    private IBulkRegistrationBuilderTarget WithRegistration(Func<Type, IRegistration> createRegistration)
    {
        _registrations.Init();
        _registrations.AddRange(_types.Select(createRegistration));

        return this;
    }
}