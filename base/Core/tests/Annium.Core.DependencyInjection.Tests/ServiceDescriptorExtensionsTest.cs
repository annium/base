using System;
using Annium.Testing;
using Xunit;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Annium.Core.DependencyInjection.Tests;

/// <summary>
/// Tests for <see cref="ServiceDescriptorExtensions.ToMicrosoft"/> conversion branches
/// and the generic factory overloads on <see cref="ServiceDescriptor"/>.
/// </summary>
public class ServiceDescriptorExtensionsTest
{
    // -----------------------------------------------------------------------
    // Group 2 — ToMicrosoft() branches
    // -----------------------------------------------------------------------

    /// <summary>
    /// Verifies that a non-keyed type descriptor round-trips correctly through ToMicrosoft().
    /// </summary>
    [Fact]
    public void ToMicrosoft_Type_PreservesFields()
    {
        // arrange
        var descriptor = ServiceDescriptor.Type(typeof(object), typeof(object), ServiceLifetime.Transient);

        // act
        var ms = descriptor.ToMicrosoft();

        // assert
        ms.ServiceType.Is(typeof(object));
        ms.ImplementationType.Is(typeof(object));
        ms.ServiceKey.IsDefault();
        ms.Lifetime.Is(MicrosoftServiceLifetime.Transient);
    }

    /// <summary>
    /// Verifies that a keyed type descriptor round-trips correctly through ToMicrosoft().
    /// </summary>
    [Fact]
    public void ToMicrosoft_KeyedType_PreservesFields()
    {
        // arrange
        var descriptor = ServiceDescriptor.KeyedType(typeof(object), "myKey", typeof(object), ServiceLifetime.Scoped);

        // act
        var ms = descriptor.ToMicrosoft();

        // assert
        ms.ServiceType.Is(typeof(object));
        ms.KeyedImplementationType.Is(typeof(object));
        ms.ServiceKey.Is("myKey");
        ms.Lifetime.Is(MicrosoftServiceLifetime.Scoped);
    }

    /// <summary>
    /// Verifies that a non-keyed factory descriptor round-trips correctly through ToMicrosoft().
    /// </summary>
    [Fact]
    public void ToMicrosoft_Factory_PreservesFields()
    {
        // arrange
        Func<IServiceProvider, object> factory = static _ => new object();
        var descriptor = ServiceDescriptor.Factory(typeof(object), factory, ServiceLifetime.Singleton);

        // act
        var ms = descriptor.ToMicrosoft();

        // assert
        ms.ServiceType.Is(typeof(object));
        ms.ImplementationFactory.Is(factory);
        ms.ServiceKey.IsDefault();
        ms.Lifetime.Is(MicrosoftServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that a keyed factory descriptor round-trips correctly through ToMicrosoft().
    /// </summary>
    [Fact]
    public void ToMicrosoft_KeyedFactory_PreservesFields()
    {
        // arrange
        Func<IServiceProvider, object, object> factory = static (_, _) => new object();
        var descriptor = ServiceDescriptor.KeyedFactory(typeof(object), "k2", factory, ServiceLifetime.Transient);

        // act
        var ms = descriptor.ToMicrosoft();

        // assert
        ms.ServiceType.Is(typeof(object));
        ms.KeyedImplementationFactory.IsNotDefault();
        ms.ServiceKey.Is("k2");
        ms.Lifetime.Is(MicrosoftServiceLifetime.Transient);
    }

    /// <summary>
    /// Verifies that a non-keyed instance descriptor round-trips correctly through ToMicrosoft().
    /// </summary>
    [Fact]
    public void ToMicrosoft_Instance_PreservesFields()
    {
        // arrange
        var instance = new object();
        var descriptor = ServiceDescriptor.Instance(typeof(object), instance, ServiceLifetime.Singleton);

        // act
        var ms = descriptor.ToMicrosoft();

        // assert
        ms.ServiceType.Is(typeof(object));
        ms.ImplementationInstance.Is(instance);
        ms.ServiceKey.IsDefault();
        ms.Lifetime.Is(MicrosoftServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that a keyed instance descriptor round-trips correctly through ToMicrosoft().
    /// </summary>
    [Fact]
    public void ToMicrosoft_KeyedInstance_PreservesFields()
    {
        // arrange
        var instance = new object();
        var descriptor = ServiceDescriptor.KeyedInstance(typeof(object), "k3", instance, ServiceLifetime.Singleton);

        // act
        var ms = descriptor.ToMicrosoft();

        // assert
        ms.ServiceType.Is(typeof(object));
        ms.KeyedImplementationInstance.Is(instance);
        ms.ServiceKey.Is("k3");
        ms.Lifetime.Is(MicrosoftServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that an unsupported IServiceDescriptor implementation causes ToMicrosoft()
    /// to throw <see cref="NotSupportedException"/>.
    /// </summary>
    [Fact]
    public void ToMicrosoft_UnsupportedDescriptor_Throws()
    {
        // arrange — a custom IServiceDescriptor that does not match any known sub-type
        IServiceDescriptor unsupported = new UnsupportedDescriptor();

        // act & assert
        Wrap.It(() => unsupported.ToMicrosoft()).Throws<NotSupportedException>();
    }

    // -----------------------------------------------------------------------
    // Group 3 — ServiceDescriptor generic overloads
    // -----------------------------------------------------------------------

    /// <summary>
    /// Verifies that Type&lt;TService,TImplementation&gt; captures both type arguments.
    /// </summary>
    [Fact]
    public void ServiceDescriptor_TypeGeneric_PreservesTypeArguments()
    {
        // act
        var d = ServiceDescriptor.Type<IFoo, Foo>(ServiceLifetime.Singleton);

        // assert
        d.ServiceType.Is(typeof(IFoo));
        d.ImplementationType.Is(typeof(Foo));
        d.Lifetime.Is(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that KeyedType&lt;TService,TImplementation&gt; captures both type arguments and the key.
    /// </summary>
    [Fact]
    public void ServiceDescriptor_KeyedTypeGeneric_PreservesTypeArguments()
    {
        // act
        var d = ServiceDescriptor.KeyedType<IFoo, Foo>("keyT", ServiceLifetime.Scoped);

        // assert
        d.ServiceType.Is(typeof(IFoo));
        d.ImplementationType.Is(typeof(Foo));
        d.Key.Is("keyT");
        d.Lifetime.Is(ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Verifies that Factory&lt;T&gt; records ServiceType correctly.
    /// </summary>
    [Fact]
    public void ServiceDescriptor_FactoryGeneric_PreservesTypeArgument()
    {
        // act
        var d = ServiceDescriptor.Factory<Foo>(static _ => new Foo(), ServiceLifetime.Singleton);

        // assert
        d.ServiceType.Is(typeof(Foo));
        d.Lifetime.Is(ServiceLifetime.Singleton);
        d.ImplementationFactory.IsNotDefault();
    }

    /// <summary>
    /// Verifies that KeyedFactory&lt;T&gt; records ServiceType and Key correctly.
    /// </summary>
    [Fact]
    public void ServiceDescriptor_KeyedFactoryGeneric_PreservesTypeArgument()
    {
        // act
        var d = ServiceDescriptor.KeyedFactory<Foo>("kf", static (_, _) => new Foo(), ServiceLifetime.Transient);

        // assert
        d.ServiceType.Is(typeof(Foo));
        d.Key.Is("kf");
        d.Lifetime.Is(ServiceLifetime.Transient);
        d.ImplementationFactory.IsNotDefault();
    }

    /// <summary>
    /// Verifies that Instance&lt;T&gt; records ServiceType and the instance correctly.
    /// </summary>
    [Fact]
    public void ServiceDescriptor_InstanceGeneric_PreservesTypeArgument()
    {
        // arrange
        var obj = new Foo();

        // act
        var d = ServiceDescriptor.Instance<Foo>(obj, ServiceLifetime.Singleton);

        // assert
        d.ServiceType.Is(typeof(Foo));
        d.ImplementationInstance.Is(obj);
        d.Lifetime.Is(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that KeyedInstance&lt;T&gt; records ServiceType, Key, and the instance correctly.
    /// </summary>
    [Fact]
    public void ServiceDescriptor_KeyedInstanceGeneric_PreservesTypeArgument()
    {
        // arrange
        var obj = new Foo();

        // act
        var d = ServiceDescriptor.KeyedInstance<Foo>("ki", obj, ServiceLifetime.Singleton);

        // assert
        d.ServiceType.Is(typeof(Foo));
        d.Key.Is("ki");
        d.ImplementationInstance.Is(obj);
        d.Lifetime.Is(ServiceLifetime.Singleton);
    }

    // -----------------------------------------------------------------------
    // Nested test types
    // -----------------------------------------------------------------------

    /// <summary>
    /// Minimal service interface used as the service type in generic overload tests.
    /// </summary>
    private interface IFoo;

    /// <summary>
    /// Minimal implementation class used in generic overload tests.
    /// </summary>
    private sealed class Foo : IFoo;

    /// <summary>
    /// A custom <see cref="IServiceDescriptor"/> implementation that is not one of the six
    /// known sub-types. Used to exercise the <c>_ => throw NotSupportedException</c> branch
    /// in <see cref="ServiceDescriptorExtensions.ToMicrosoft"/>.
    /// </summary>
    private sealed class UnsupportedDescriptor : IServiceDescriptor
    {
        /// <summary>The service type — dummy value; unused by the test, only the descriptor identity matters.</summary>
        public Type ServiceType => typeof(object);

        /// <summary>The service key — always <see langword="null"/> for this dummy descriptor.</summary>
        public object? Key => null;

        /// <summary>The service lifetime — dummy value; <see cref="ServiceLifetime.Singleton"/> chosen arbitrarily.</summary>
        public ServiceLifetime Lifetime => ServiceLifetime.Singleton;
    }
}
