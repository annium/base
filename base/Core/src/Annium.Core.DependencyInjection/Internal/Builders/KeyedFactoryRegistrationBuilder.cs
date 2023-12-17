using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

internal class KeyedFactoryRegistrationBuilder : IKeyedFactoryRegistrationBuilderBase
{
    private readonly IServiceContainer _container;
    private readonly Type _type;
    private readonly Func<IServiceProvider, object, object> _factory;
    private readonly Registrar _registrar;
    private readonly RegistrationsCollection _registrations = new();

    public KeyedFactoryRegistrationBuilder(
        IServiceContainer container,
        Type type,
        Func<IServiceProvider, object, object> factory,
        Registrar registrar
    )
    {
        _container = container;
        _type = type;
        _factory = factory;
        _registrar = registrar;
    }

    public IKeyedFactoryRegistrationBuilderBase AsKeyedSelf(object key) =>
        WithRegistration(new KeyedFactoryRegistration(_type, key, _factory));

    public IKeyedFactoryRegistrationBuilderBase AsKeyed(Type serviceType, object key) =>
        WithRegistration(new KeyedFactoryRegistration(serviceType, key, _factory));

    public IKeyedFactoryRegistrationBuilderBase AsKeyedInterfaces(object key) =>
        WithRegistrations(_type.GetInterfaces().Select(x => new KeyedFactoryRegistration(x, key, _factory)));

    public IServiceContainer In(ServiceLifetime lifetime)
    {
        _registrar.Register(_registrations, lifetime);

        return _container;
    }

    public IServiceContainer Scoped() => In(ServiceLifetime.Scoped);

    public IServiceContainer Singleton() => In(ServiceLifetime.Singleton);

    public IServiceContainer Transient() => In(ServiceLifetime.Transient);

    private IKeyedFactoryRegistrationBuilderBase WithRegistrations(IEnumerable<IRegistration> registrations)
    {
        _registrations.Init();
        _registrations.AddRange(registrations);

        return this;
    }

    private IKeyedFactoryRegistrationBuilderBase WithRegistration(IRegistration registration)
    {
        _registrations.Init();
        _registrations.Add(registration);

        return this;
    }
}
