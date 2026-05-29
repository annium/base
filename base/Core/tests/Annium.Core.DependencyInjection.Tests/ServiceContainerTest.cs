using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests;

/// <summary>
/// Tests for service container functionality
/// </summary>
public class ServiceContainerTest : TestBase
{
    public ServiceContainerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that adding a service to the container writes to collection immediately
    /// </summary>
    [Fact]
    public void Add_WritesToCollectionImmediately()
    {
        // arrange
        var instance = new A();

        // act
        Container.Add(instance).AsSelf().Singleton();
        Build();

        // assert
        Get<A>().Is(instance);
    }

    /// <summary>
    /// Verifies that the container correctly identifies when it contains a type descriptor
    /// </summary>
    [Fact]
    public void ContainsType_Works()
    {
        // arrange
        Container.Add<A>().AsSelf().Singleton();

        // assert
        Container.Contains(ServiceDescriptor.Type(typeof(A), typeof(A), ServiceLifetime.Singleton)).IsTrue();
    }

    /// <summary>
    /// Verifies that the container correctly identifies when it contains a factory descriptor
    /// </summary>
    [Fact]
    public void ContainsFactory_Works()
    {
        // arrange
        static B Factory(IServiceProvider _) => new();
        Container.Add(Factory).AsSelf().Singleton();

        // assert
        Container.Contains(ServiceDescriptor.Factory(typeof(B), Factory, ServiceLifetime.Singleton)).IsTrue();
    }

    /// <summary>
    /// Verifies that the container correctly identifies when it contains an instance descriptor
    /// </summary>
    [Fact]
    public void ContainsInstance_Works()
    {
        // arrange
        var instance = new B();
        Container.Add(instance).AsSelf().Singleton();

        // assert
        Container.Contains(ServiceDescriptor.Instance(typeof(B), instance, ServiceLifetime.Singleton)).IsTrue();
    }

    /// <summary>
    /// Verifies that the container correctly identifies when it contains a keyed type descriptor
    /// added directly via <see cref="IServiceContainer.Add(IServiceDescriptor)"/>.
    /// </summary>
    [Fact]
    public void Contains_KeyedTypeDescriptor()
    {
        // arrange — add a raw keyed-type descriptor so Contains can match it exactly
        IServiceDescriptor descriptor = ServiceDescriptor.KeyedType(
            typeof(A),
            "key",
            typeof(B),
            ServiceLifetime.Singleton
        );
        Container.Add(descriptor);

        // assert — matching descriptor is found
        Container
            .Contains(ServiceDescriptor.KeyedType(typeof(A), "key", typeof(B), ServiceLifetime.Singleton))
            .IsTrue();
        // non-matching key returns false
        Container
            .Contains(ServiceDescriptor.KeyedType(typeof(A), "other", typeof(B), ServiceLifetime.Singleton))
            .IsFalse();
    }

    /// <summary>
    /// Verifies that the container correctly identifies when it contains a keyed factory descriptor
    /// added directly via <see cref="IServiceContainer.Add(IServiceDescriptor)"/>.
    /// The factory-delegate identity check (Method + Target) requires the same delegate be used
    /// when constructing the check descriptor.
    /// </summary>
    [Fact]
    public void Contains_KeyedFactoryDescriptor()
    {
        // arrange — stable delegate identity: use a stored delegate reference
        static object KeyedFactory(IServiceProvider _, object __) => new B();
        IServiceDescriptor descriptor = ServiceDescriptor.KeyedFactory(
            typeof(B),
            "key",
            KeyedFactory,
            ServiceLifetime.Singleton
        );
        Container.Add(descriptor);

        // assert — matching descriptor (same delegate Method+Target and same key) is found
        Container
            .Contains(ServiceDescriptor.KeyedFactory(typeof(B), "key", KeyedFactory, ServiceLifetime.Singleton))
            .IsTrue();
        // non-matching key returns false
        Container
            .Contains(ServiceDescriptor.KeyedFactory(typeof(B), "other", KeyedFactory, ServiceLifetime.Singleton))
            .IsFalse();
    }

    /// <summary>
    /// Verifies that the container correctly identifies when it contains a keyed instance descriptor
    /// added directly via <see cref="IServiceContainer.Add(IServiceDescriptor)"/>.
    /// </summary>
    [Fact]
    public void Contains_KeyedInstanceDescriptor()
    {
        // arrange — add a raw keyed-instance descriptor
        var instance = new B();
        IServiceDescriptor descriptor = ServiceDescriptor.KeyedInstance(
            typeof(A),
            "key",
            instance,
            ServiceLifetime.Singleton
        );
        Container.Add(descriptor);

        // assert — matching descriptor is found
        Container
            .Contains(ServiceDescriptor.KeyedInstance(typeof(A), "key", instance, ServiceLifetime.Singleton))
            .IsTrue();
        // different instance returns false
        Container
            .Contains(ServiceDescriptor.KeyedInstance(typeof(A), "key", new B(), ServiceLifetime.Singleton))
            .IsFalse();
    }

    /// <summary>
    /// Verifies that OnBuild subscribers are invoked with the built provider
    /// </summary>
    [Fact]
    public void OnBuild_SubscribersAreInvoked()
    {
        // arrange
        IServiceProvider? captured = null;
        Container.OnBuild += sp => captured = sp;

        // act
        var built = Container.BuildServiceProvider();

        // assert
        captured.Is(built);
        built.Dispose();
    }

    /// <summary>
    /// Verifies that Clone produces a container with the same number of descriptors as the original
    /// </summary>
    [Fact]
    public void Clone_DescriptorsAreCopied()
    {
        // arrange
        Container.Add<A>().AsSelf().Singleton();
        var countBefore = Container.Count;

        // act
        var clone = Container.Clone();

        // assert
        clone.Count.Is(countBefore);
        clone.HasSingleton(typeof(A), typeof(A));
    }

    /// <summary>
    /// Verifies that OnBuild subscribers are not propagated to a clone — building the clone does not fire original subscribers
    /// </summary>
    [Fact]
    public void Clone_OnBuildNotPropagated()
    {
        // arrange
        var originalFired = false;
        Container.OnBuild += _ => originalFired = true;
        var clone = Container.Clone();

        // act — build clone: original subscriber must NOT fire
        var cloneProvider = clone.BuildServiceProvider();
        cloneProvider.Dispose();

        originalFired.IsFalse();

        // build original: subscriber MUST fire
        var originalProvider = Container.BuildServiceProvider();
        originalProvider.Dispose();

        originalFired.IsTrue();
    }

    /// <summary>
    /// Verifies that Add&lt;TService, TImplementation&gt;() registers TImplementation mapped to TService
    /// </summary>
    [Fact]
    public void Add_GenericTwoType_Works()
    {
        // arrange
        Container.Add<IFoo, Foo>().Singleton();

        // act
        Build();

        // assert
        Get<IFoo>().AsExact<Foo>();
    }

    /// <summary>
    /// Verifies that Add&lt;TService, TImplementation&gt;(key) registers TImplementation as a keyed TService
    /// </summary>
    [Fact]
    public void Add_GenericTwoTypeKeyed_Works()
    {
        // arrange
        Container.Add<IFoo, Foo>("k").Singleton();

        // act
        Build();

        // assert
        GetKeyed<IFoo>("k").AsExact<Foo>();
    }

    /// <summary>
    /// Verifies that TryResolve returns the registered instance when the service is in the container
    /// </summary>
    [Fact]
    public void TryResolve_Registered_ReturnsInstance()
    {
        // arrange
        Container.Add<A>().AsSelf().Singleton();
        Build();

        // act
        var result = Provider.TryResolve<A>();

        // assert
        result.IsNotDefault();
        result.AsExact<A>();
    }

    /// <summary>
    /// Verifies that TryResolve returns null when the service is not registered
    /// </summary>
    [Fact]
    public void TryResolve_Unregistered_ReturnsNull()
    {
        // arrange — build empty container (no A registered)
        Build();

        // act
        var result = Provider.TryResolve<A>();

        // assert
        result.IsDefault();
    }

    /// <summary>
    /// Verifies that TryResolveKeyed returns the registered keyed instance
    /// </summary>
    [Fact]
    public void TryResolveKeyed_Registered_ReturnsInstance()
    {
        // arrange
        Container.Add<IFoo, Foo>("k").Singleton();
        Build();

        // act
        var result = Provider.TryResolveKeyed<IFoo>("k");

        // assert
        result.IsNotDefault();
        result.AsExact<Foo>();
    }

    /// <summary>
    /// Verifies that TryResolveKeyed returns null when the keyed service is not registered
    /// </summary>
    [Fact]
    public void TryResolveKeyed_Unregistered_ReturnsNull()
    {
        // arrange — build empty container
        Build();

        // act
        var result = Provider.TryResolveKeyed<IFoo>("missing");

        // assert
        result.IsDefault();
    }

    /// <summary>
    /// Verifies that ServiceDescriptor.Instance throws when a non-Singleton lifetime is supplied
    /// </summary>
    [Fact]
    public void ServiceDescriptor_Instance_NonSingleton_Throws()
    {
        Wrap.It(() => ServiceDescriptor.Instance(typeof(object), new object(), ServiceLifetime.Scoped))
            .Throws<ArgumentException>();
    }

    /// <summary>
    /// Verifies that ServiceDescriptor.KeyedInstance throws when a non-Singleton lifetime is supplied
    /// </summary>
    [Fact]
    public void ServiceDescriptor_KeyedInstance_NonSingleton_Throws()
    {
        Wrap.It(() => ServiceDescriptor.KeyedInstance(typeof(object), "k", new object(), ServiceLifetime.Transient))
            .Throws<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Resolve(Type) returns the registered instance when the service is in the container
    /// </summary>
    [Fact]
    public void ResolveType_Registered_ReturnsInstance()
    {
        // arrange
        var instance = new B();
        Container.Add(instance).AsSelf().Singleton();
        Build();

        // act
        var result = Provider.Resolve(typeof(B));

        // assert
        ((B)result).Is(instance);
    }

    /// <summary>
    /// Verifies that TryResolve(Type) returns null when the service is not registered
    /// </summary>
    [Fact]
    public void TryResolveType_Unregistered_ReturnsNull()
    {
        // arrange — build empty container (no B registered)
        Build();

        // act
        var result = Provider.TryResolve(typeof(B));

        // assert
        result.IsDefault();
    }

    /// <summary>
    /// Verifies that ResolveKeyed(Type, key) returns the registered keyed instance
    /// </summary>
    [Fact]
    public void ResolveKeyedType_Registered_ReturnsInstance()
    {
        // arrange
        Container.Add<IFoo, Foo>("k").Singleton();
        Build();

        // act
        var result = Provider.ResolveKeyed(typeof(IFoo), "k");

        // assert
        result.IsNotDefault();
        result.AsExact<Foo>();
    }

    /// <summary>
    /// Verifies that TryResolveKeyed(Type, key) returns null when the keyed service is not registered
    /// </summary>
    [Fact]
    public void TryResolveKeyedType_Unregistered_ReturnsNull()
    {
        // arrange — build empty container
        Build();

        // act
        var result = Provider.TryResolveKeyed(typeof(IFoo), "k");

        // assert
        result.IsDefault();
    }

    /// <summary>
    /// Test record B that inherits from A
    /// </summary>
    private sealed record B : A;

    /// <summary>
    /// Test record A
    /// </summary>
    private record A;

    /// <summary>
    /// Test interface IFoo
    /// </summary>
    private interface IFoo;

    /// <summary>
    /// Test class Foo implementing IFoo
    /// </summary>
    private sealed class Foo : IFoo;
}
