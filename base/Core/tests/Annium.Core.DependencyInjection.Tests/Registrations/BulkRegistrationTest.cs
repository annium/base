using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Builders;
using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

/// <summary>
/// Tests for bulk registration functionality in the dependency injection container
/// </summary>
public class BulkRegistrationTest : TestBase
{
    /// <summary>
    /// Verifies that filtering types with Where clause during bulk registration works correctly
    /// </summary>
    [Fact]
    public void Where_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).Where(x => x == typeof(A)).AsSelf().Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
    }

    /// <summary>
    /// Verifies that registering types as themselves during bulk registration works correctly
    /// </summary>
    [Fact]
    public void AsSelf_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).AsSelf().Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        Get<A>().AsExact<A>();
        Get<B>().AsExact<B>();
    }

    /// <summary>
    /// Verifies that registering types as a specific type during bulk registration works correctly
    /// </summary>
    [Fact]
    public void As_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).As(typeof(A)).Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        Container.HasSingletonTypeFactory(typeof(A));
        Get<A>().Is(Get<B>());
        Get<IEnumerable<A>>().At(0).AsExact<A>();
        Get<IEnumerable<A>>().At(1).AsExact<B>();
    }

    /// <summary>
    /// Verifies that registering types as their interfaces during bulk registration works correctly
    /// </summary>
    [Fact]
    public void AsInterfaces_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).AsInterfaces().Singleton();

        // act
        Build();

        // assert
        Get<IA>().Is(Get<IB>());
        Get<IEnumerable<IA>>().At(0).AsExact<A>();
        Get<IEnumerable<IA>>().At(1).AsExact<B>();
    }

    /// <summary>
    /// Verifies that registering types as keyed self during bulk registration works correctly
    /// </summary>
    [Fact]
    public void AsKeyedSelf_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).AsKeyedSelf(x => x.Name).Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        Get<A>().AsExact<A>();
        Get<B>().AsExact<B>();
        GetKeyed<A>(nameof(A)).Is(Get<A>());
        GetKeyed<B>(nameof(B)).Is(Get<B>());
    }

    /// <summary>
    /// Verifies that registering types as keyed services during bulk registration works correctly
    /// </summary>
    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).AsKeyed(typeof(A), x => x.Name).Singleton();

        // act
        Build();

        // assert
        GetKeyed<A>(nameof(A)).AsExact<A>();
        GetKeyed<A>(nameof(B)).AsExact<B>();
    }

    /// <summary>
    /// Verifies that registering types as self factories during bulk registration works correctly
    /// </summary>
    [Fact]
    public void AsSelfFactory_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).AsSelfFactory().Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        Container.HasSingletonFuncFactory(typeof(A));
        Container.HasSingletonFuncFactory(typeof(B));
        Get<Func<A>>()().Is(Get<A>());
        Get<Func<B>>()().Is(Get<B>());
    }

    /// <summary>
    /// Verifies that registering types as factories during bulk registration works correctly
    /// </summary>
    [Fact]
    public void AsFactory_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).AsFactory<A>().Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        Container.HasSingletonFuncFactory(typeof(A), 2);
        Get<IEnumerable<Func<A>>>().At(0)().AsExact<A>();
        Get<IEnumerable<Func<A>>>().At(1)().AsExact<B>();
    }

    /// <summary>
    /// Verifies that registering types as keyed self factories during bulk registration works correctly
    /// </summary>
    [Fact]
    public void AsKeyedSelfFactory_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).AsKeyedSelfFactory(x => x.Name).Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        GetKeyed<Func<A>>(nameof(A))().AsExact<A>();
        GetKeyed<Func<B>>(nameof(B))().AsExact<B>();
    }

    /// <summary>
    /// Verifies that registering types as keyed factories during bulk registration works correctly
    /// </summary>
    [Fact]
    public void AsKeyedFactory_Works()
    {
        // arrange
        Container
            .Add(new[] { typeof(A), typeof(B) }.AsEnumerable())
            .AsKeyedFactory(typeof(A), x => x.Name)
            .Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        GetKeyed<Func<A>>(nameof(A))().AsExact<A>();
        GetKeyed<Func<A>>(nameof(B))().AsExact<B>();
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
