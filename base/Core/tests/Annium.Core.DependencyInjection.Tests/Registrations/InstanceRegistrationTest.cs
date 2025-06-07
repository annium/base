using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

/// <summary>
/// Tests for instance registration functionality in the dependency injection container
/// </summary>
public class InstanceRegistrationTest : TestBase
{
    /// <summary>
    /// Verifies that instance registration as self works correctly
    /// </summary>
    [Fact]
    public void AsSelf_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsSelf().Singleton();

        // act
        Build();

        // assert
        Get<D>().Is(instance);
    }

    /// <summary>
    /// Verifies that instance registration as specific type works correctly
    /// </summary>
    [Fact]
    public void As_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).As(typeof(A)).Singleton();

        // act
        Build();

        // assert
    }

    /// <summary>
    /// Verifies that instance registration as interfaces works correctly
    /// </summary>
    [Fact]
    public void AsInterfaces_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsSelf().AsInterfaces().Singleton();

        // act
        Build();

        // assert
        Get<IC>().Is(Get<D>());
        Get<ID>().Is(Get<D>());
    }

    /// <summary>
    /// Verifies that instance registration as keyed self works correctly
    /// </summary>
    [Fact]
    public void AsKeyedSelf_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsKeyedSelf(nameof(D)).Singleton();

        // act
        Build();

        // assert
        GetKeyed<D>(nameof(D)).Is(instance);
    }

    /// <summary>
    /// Verifies that instance registration as keyed service works correctly
    /// </summary>
    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsKeyed<C>(nameof(D)).Singleton();

        // act
        Build();

        // assert
        GetKeyed<C>(nameof(D)).Is(instance);
    }

    /// <summary>
    /// Verifies that instance registration as self factory works correctly
    /// </summary>
    [Fact]
    public void AsSelfFactory_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsSelfFactory().Singleton();

        // act
        Build();

        // assert
        Get<Func<D>>()().Is(instance);
    }

    /// <summary>
    /// Verifies that instance registration as factory works correctly
    /// </summary>
    [Fact]
    public void AsFactory_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsFactory<C>().Singleton();

        // act
        Build();

        // assert
        Get<Func<C>>()().Is(instance);
    }

    /// <summary>
    /// Verifies that instance registration as keyed self factory works correctly
    /// </summary>
    [Fact]
    public void AsKeyedSelfFactory_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsKeyedSelfFactory(nameof(D)).Singleton();

        // act
        Build();

        // assert
        GetKeyed<Func<D>>(nameof(D))().Is(instance);
    }

    /// <summary>
    /// Verifies that instance registration as keyed factory works correctly
    /// </summary>
    [Fact]
    public void AsKeyedFactory_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsKeyedFactory<C>(nameof(C)).Singleton();

        // act
        Build();

        // assert
        GetKeyed<Func<C>>(nameof(C))().Is(instance);
    }

    /// <summary>
    /// Test class D that inherits from C and implements ID
    /// </summary>
    private sealed class D : C, ID
    {
        public D(A x)
            : base(x) { }
    }

    /// <summary>
    /// Test class C that implements IC
    /// </summary>
    private class C : IC
    {
        // ReSharper disable once UnusedParameter.Local
        protected C(A _) { }
    }

    /// <summary>
    /// Test interface ID that extends IC
    /// </summary>
    private interface ID : IC;

    /// <summary>
    /// Test interface IC
    /// </summary>
    private interface IC;

    /// <summary>
    /// Test class A
    /// </summary>
    private class A;
}
