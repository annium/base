using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

/// <summary>
/// Tests for single type registration functionality in the dependency injection container
/// </summary>
public class SingleRegistrationTest : TestBase
{
    /// <summary>
    /// Verifies that single type registration as self works correctly
    /// </summary>
    [Fact]
    public void AsSelf_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsSelf().Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(B), typeof(B));
        Get<B>().AsExact<B>();
    }

    /// <summary>
    /// Verifies that single type registration as specific type works correctly
    /// </summary>
    [Fact]
    public void As_Works()
    {
        // arrange
        Container.Add(typeof(B)).As(typeof(A)).Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonTypeFactory(typeof(A));
        Get<A>().Is(Get<B>());
    }

    /// <summary>
    /// Verifies that single type registration as interfaces works correctly
    /// </summary>
    [Fact]
    public void AsInterfaces_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsInterfaces().Singleton();

        // act
        Build();

        // assert
        Get<IA>().Is(Get<IB>());
        Get<IB>().Is(Get<IB>());
    }

    /// <summary>
    /// Verifies that single type registration as keyed self works correctly
    /// </summary>
    [Fact]
    public void AsKeyedSelf_Works()
    {
        // arrange
        B.Reset();
        Container.Add(typeof(B)).AsKeyedSelf(nameof(B)).Singleton();

        // act
        Build();

        // assert
        B.InstancesCount.Is(0);
        GetKeyed<B>(nameof(B)).Is(Get<B>());
        B.InstancesCount.Is(1);
    }

    /// <summary>
    /// Verifies that single type registration as keyed service works correctly
    /// </summary>
    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        B.Reset();
        Container.Add(typeof(B)).AsKeyed(typeof(A), nameof(B)).Singleton();

        // act
        Build();

        // assert
        B.InstancesCount.Is(0);
        GetKeyed<A>(nameof(B)).Is(Get<B>());
        B.InstancesCount.Is(1);
    }

    /// <summary>
    /// Verifies that single type registration as self factory works correctly
    /// </summary>
    [Fact]
    public void AsSelfFactory_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsSelfFactory().Singleton();

        // act
        Build();

        // assert
        Get<Func<B>>()().Is(Get<B>());
    }

    /// <summary>
    /// Verifies that single type registration as factory works correctly
    /// </summary>
    [Fact]
    public void AsFactory_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsFactory<A>().Singleton();

        // act
        Build();

        // assert
        Get<Func<A>>()().Is(Get<B>());
    }

    /// <summary>
    /// Verifies that single type registration as keyed self factory works correctly
    /// </summary>
    [Fact]
    public void AsKeyedSelfFactory_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsKeyedSelfFactory(nameof(B)).Singleton();

        // act
        Build();

        // assert
        GetKeyed<Func<B>>(nameof(B))().Is(Get<B>());
    }

    /// <summary>
    /// Verifies that single type registration as keyed factory works correctly
    /// </summary>
    [Fact]
    public void AsKeyedFactory_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsKeyedFactory(typeof(A), nameof(B)).Singleton();

        // act
        Build();

        // assert
        GetKeyed<Func<A>>(nameof(B))().Is(Get<B>());
    }

    /// <summary>
    /// Test class B that inherits from A and implements IB
    /// </summary>
    private sealed class B : A, IB
    {
        /// <summary>
        /// Resets the instance count for testing purposes
        /// </summary>
        public static void Reset()
        {
            InstancesCount = 0;
        }

        /// <summary>
        /// Gets the number of instances created
        /// </summary>
        public static int InstancesCount { get; private set; }

        public B()
        {
            InstancesCount++;
        }
    }

    /// <summary>
    /// Test class A that implements IA
    /// </summary>
    private class A : IA;

    /// <summary>
    /// Test interface IB that extends IA
    /// </summary>
    private interface IB : IA;

    /// <summary>
    /// Test interface IA
    /// </summary>
    private interface IA;
}
