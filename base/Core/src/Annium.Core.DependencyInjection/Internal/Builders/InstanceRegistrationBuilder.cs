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

    public InstanceRegistrationBuilder(IServiceContainer container, Type type, object instance, Registrar registrar)
    {
        _container = container;
        _type = type;
        _instance = instance;
        _registrar = registrar;
    }

    public IInstanceRegistrationBuilderBase AsSelf() => WithRegistration(new InstanceRegistration(_type, _instance));

    public IInstanceRegistrationBuilderBase As(Type serviceType) =>
        WithRegistration(new InstanceRegistration(serviceType, _instance));

    public IInstanceRegistrationBuilderBase AsInterfaces() =>
        WithRegistrations(_type.GetInterfaces().Select(x => new InstanceRegistration(x, _instance)));

    public IInstanceRegistrationBuilderBase AsKeyedSelf(object key) =>
        WithRegistration(new KeyedInstanceRegistration(_type, key, _instance));

    public IInstanceRegistrationBuilderBase AsKeyed(Type serviceType, object key) =>
        WithRegistration(new KeyedInstanceRegistration(serviceType, key, _instance));

    public IInstanceRegistrationBuilderBase AsSelfFactory() =>
        WithRegistration(new InstanceFactoryRegistration(_type, _instance));

    public IInstanceRegistrationBuilderBase AsFactory(Type serviceType) =>
        WithRegistration(new InstanceFactoryRegistration(serviceType, _instance));

    public IInstanceRegistrationBuilderBase AsKeyedSelfFactory(object key) =>
        WithRegistration(new KeyedInstanceFactoryRegistration(_type, key, _instance));

    public IInstanceRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key) =>
        WithRegistration(new KeyedInstanceFactoryRegistration(serviceType, key, _instance));

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
