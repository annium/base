using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

internal class InstanceRegistrationBuilder : IInstanceRegistrationBuilderBase
{
    private readonly IServiceContainer _container;
    private readonly Type _type;
    private readonly object _instance;
    private readonly Registrar _registrar;
    private readonly RegistrationsCollection _registrations = new();

    public InstanceRegistrationBuilder(
        IServiceContainer container,
        Type type,
        object instance,
        Registrar registrar
    )
    {
        _container = container;
        _type = type;
        _instance = instance;
        _registrar = registrar;
    }

    public IInstanceRegistrationBuilderBase AsSelf() =>
        WithRegistration(new InstanceRegistration(_type, _instance));

    public IInstanceRegistrationBuilderBase As(Type serviceType) =>
        WithRegistration(new InstanceRegistration(serviceType, _instance));

    public IInstanceRegistrationBuilderBase AsInterfaces() =>
        WithRegistrations(_type.GetInterfaces().Select(x => new InstanceRegistration(x, _instance)));

    public IInstanceRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull =>
        WithRegistration(new InstanceKeyedRegistration(_type, _instance, typeof(TKey), key));

    public IInstanceRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull =>
        WithRegistration(new InstanceKeyedRegistration(serviceType, _instance, typeof(TKey), key));

    public IInstanceRegistrationBuilderBase AsSelfFactory() =>
        WithRegistration(new InstanceFactoryRegistration(_type, _instance));

    public IInstanceRegistrationBuilderBase AsFactory(Type serviceType) =>
        WithRegistration(new InstanceFactoryRegistration(serviceType, _instance));

    public IInstanceRegistrationBuilderBase AsKeyedSelfFactory<TKey>(TKey key) where TKey : notnull =>
        WithRegistration(new InstanceKeyedFactoryRegistration(_type, _instance, typeof(TKey), key));

    public IInstanceRegistrationBuilderBase AsKeyedFactory<TKey>(Type serviceType, TKey key) where TKey : notnull =>
        WithRegistration(new InstanceKeyedFactoryRegistration(serviceType, _instance, typeof(TKey), key));

    public IServiceContainer Singleton()
    {
        _registrar.Register(_registrations, ServiceLifetime.Singleton);

        return _container;
    }

    private IInstanceRegistrationBuilderBase WithRegistrations(IEnumerable<IRegistration> registrations)
    {
        _registrations.Init();
        _registrations.AddRange(registrations);

        return this;
    }

    private IInstanceRegistrationBuilderBase WithRegistration(IRegistration registration)
    {
        _registrations.Init();
        _registrations.Add(registration);

        return this;
    }
}