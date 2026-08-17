using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Internal.Builders;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

/// <summary>
/// Tests for single type registration functionality in the dependency injection container
/// </summary>
public class SingleRegistrationTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleRegistrationTest"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public SingleRegistrationTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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
    /// Verifies that single type registration as keyed interfaces works correctly
    /// </summary>
    [Fact]
    public void AsKeyedInterfaces_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsKeyedInterfaces(nameof(B)).Singleton();

        // act
        Build();

        // assert
        GetKeyed<IA>(nameof(B)).AsExact<B>();
        GetKeyed<IB>(nameof(B)).AsExact<B>();
        GetKeyed<IA>(nameof(B)).Is(GetKeyed<IB>(nameof(B)));
    }

    /// <summary>
    /// Verifies that an open-generic type registered AsSelf is stored as a type descriptor
    /// (not a factory) so it can be used as the basis for closed-generic resolution
    /// </summary>
    [Fact]
    public void AsSelf_OpenGeneric_RegistersTypeDescriptor()
    {
        // arrange
        Container.Add(typeof(List<>)).AsSelf().Singleton();

        // assert — the open-generic descriptor exists before building
        Container.HasSingleton(typeof(List<>), typeof(List<>));
    }

    /// <summary>
    /// Verifies that calling a lifetime terminator without specifying registration targets throws
    /// </summary>
    [Fact]
    public void In_NoRegistrationTargets_Throws()
    {
        // arrange & act & assert
        Wrap.It(() => Container.Add(typeof(B)).Singleton())
            .Throws<InvalidOperationException>()
            .Reports("Specify registration targets");
    }

    /// <summary>
    /// Verifies that calling a lifetime terminator a second time on the same builder throws
    /// </summary>
    [Fact]
    public void In_RegistrarReused_Throws()
    {
        // arrange
        var b = Container.Add(typeof(B)).AsSelf();
        b.Singleton();

        // act & assert
        Wrap.It(() => b.Singleton()).Throws<InvalidOperationException>().Reports(Registrar.AlreadyRegisteredMessage);
    }

    /// <summary>
    /// Verifies that single type registration with scoped lifetime works correctly
    /// </summary>
    [Fact]
    public void Scoped_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsSelf().Scoped();

        // act
        Build();

        // assert
        Container.HasScoped(typeof(B), typeof(B));
    }

    /// <summary>
    /// Verifies that single type registration with transient lifetime works correctly
    /// </summary>
    [Fact]
    public void Transient_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsSelf().Transient();

        // act
        Build();

        // assert
        Container.HasTransient(typeof(B), typeof(B));
    }

    /// <summary>
    /// Verifies that the In(lifetime) overload accepts a ServiceLifetime argument
    /// </summary>
    [Fact]
    public void In_AcceptsLifetime()
    {
        // arrange
        Container.Add(typeof(B)).AsSelf().In(ServiceLifetime.Scoped);

        // act
        Build();

        // assert
        Container.HasScoped(typeof(B), typeof(B));
    }

    /// <summary>
    /// Verifies that explicit AsSelf followed by As does not produce a duplicate self-registration
    /// </summary>
    [Fact]
    public void AsSelfThenAs_DoesNotDoubleRegisterSelf()
    {
        // arrange
        Container.Add(typeof(B)).AsSelf().As(typeof(A)).Singleton();

        // act
        Build();

        // assert
        // exactly one descriptor mapping B to itself, no duplicate
        Container.HasSingleton(typeof(B), typeof(B));
        Container.Has(typeof(B), 1);
    }

    /// <summary>
    /// Verifies that AsInterfaces also registers the implementation type as itself so it is directly resolvable
    /// </summary>
    [Fact]
    public void AsInterfaces_TypeAlsoResolvable()
    {
        // arrange
        Container.Add(typeof(B)).AsInterfaces().Singleton();

        // act
        Build();

        // assert
        Get<B>().AsExact<B>();
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

        /// <summary>
        /// Initializes a new instance of the <see cref="B"/> class.
        /// </summary>
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
