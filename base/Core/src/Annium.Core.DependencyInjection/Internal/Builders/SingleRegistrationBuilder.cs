using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

internal class SingleRegistrationBuilder : ISingleRegistrationBuilderBase
{
    private readonly IServiceContainer _container;
    private readonly Type _type;
    private readonly Registrar _registrar;
    private readonly RegistrationsCollection _registrations = new();

    public SingleRegistrationBuilder(
        IServiceContainer container,
        Type type,
        Registrar registrar
    )
    {
        _container = container;
        _type = type;
        _registrar = registrar;
    }


    public ISingleRegistrationBuilderBase AsSelf() =>
        WithRegistration(new TypeRegistration(_type, _type));

    public ISingleRegistrationBuilderBase As(Type serviceType) =>
        WithRegistration(new TypeRegistration(serviceType, _type));

    public ISingleRegistrationBuilderBase AsInterfaces() =>
        WithRegistrations(_type.GetInterfaces().Select(x => new TypeRegistration(x, _type)));

    public ISingleRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull =>
        WithRegistration(new TypeKeyedRegistration(_type, _type, typeof(TKey), key));

    public ISingleRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull =>
        WithRegistration(new TypeKeyedRegistration(serviceType, _type, typeof(TKey), key));

    public ISingleRegistrationBuilderBase AsSelfFactory() =>
        WithRegistration(new TypeFactoryRegistration(_type, _type));

    public ISingleRegistrationBuilderBase AsFactory(Type serviceType) =>
        WithRegistration(new TypeFactoryRegistration(serviceType, _type));

    public ISingleRegistrationBuilderBase AsKeyedSelfFactory<TKey>(TKey key) where TKey : notnull =>
        WithRegistration(new TypeKeyedFactoryRegistration(_type, _type, typeof(TKey), key));

    public ISingleRegistrationBuilderBase AsKeyedFactory<TKey>(Type serviceType, TKey key) where TKey : notnull =>
        WithRegistration(new TypeKeyedFactoryRegistration(serviceType, _type, typeof(TKey), key));

    public IServiceContainer In(ServiceLifetime lifetime)
    {
        _registrations.Add(new TypeRegistration(_type, _type));
        _registrar.Register(_registrations, lifetime);

        return _container;
    }

    public IServiceContainer Scoped() => In(ServiceLifetime.Scoped);
    public IServiceContainer Singleton() => In(ServiceLifetime.Singleton);
    public IServiceContainer Transient() => In(ServiceLifetime.Transient);

    private ISingleRegistrationBuilderBase WithRegistrations(IEnumerable<IRegistration> registrations)
    {
        _registrations.Init();
        _registrations.AddRange(registrations);

        return this;
    }

    private ISingleRegistrationBuilderBase WithRegistration(IRegistration registration)
    {
        _registrations.Init();
        _registrations.Add(registration);

        return this;
    }
}