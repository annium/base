using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

/// <summary>
/// Tests for bulk registration functionality in the dependency injection container
/// </summary>
public class BulkRegistrationTest : TestBase
{
    /// <summary>Shared <c>[A, B]</c> type list used as the input to most bulk-registration tests.</summary>
    private static readonly Type[] _ab = [typeof(A), typeof(B)];

    /// <summary>Shared single-element <c>[A]</c> type list for bulk tests that need only one type.</summary>
    private static readonly Type[] _aOnly = [typeof(A)];

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkRegistrationTest"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public BulkRegistrationTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that AssignableTo&lt;T&gt; filters the bulk set to types assignable to T
    /// </summary>
    [Fact]
    public void AssignableTo_FiltersToSubtypes()
    {
        // arrange — _ab contains A (implements IA) and B (implements IA + IB); filter to IA only
        Container.Add(_ab.AsEnumerable()).AssignableTo<IA>().AsSelf().Singleton();

        // act
        Build();

        // assert — both A and B implement IA, so both are registered
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        Get<A>().AsExact<A>();
        Get<B>().AsExact<B>();
    }

    /// <summary>
    /// Verifies that StartingWith filters the bulk set to types whose names begin with the given prefix
    /// </summary>
    [Fact]
    public void StartingWith_FiltersByName()
    {
        // arrange — only type "A" starts with "A"; "B" does not
        Container.Add(_ab.AsEnumerable()).StartingWith("A").AsSelf().Singleton();

        // act
        Build();

        // assert — A is registered, B is not
        Container.HasSingleton(typeof(A), typeof(A));
        Container.Has(typeof(B), 0);
    }

    /// <summary>
    /// Verifies that EndingWith filters the bulk set to types whose names end with the given suffix
    /// </summary>
    [Fact]
    public void EndingWith_FiltersByName()
    {
        // arrange — only type "B" ends with "B"; "A" does not
        Container.Add(_ab.AsEnumerable()).EndingWith("B").AsSelf().Singleton();

        // act
        Build();

        // assert — B is registered, A is not
        Container.HasSingleton(typeof(B), typeof(B));
        Container.Has(typeof(A), 0);
    }

    /// <summary>
    /// Verifies that filtering types with Where clause during bulk registration works correctly
    /// </summary>
    [Fact]
    public void Where_Works()
    {
        // arrange
        Container.Add(_ab.AsEnumerable()).Where(x => x == typeof(A)).AsSelf().Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.Has(typeof(B), 0);
    }

    /// <summary>
    /// Verifies that registering types as themselves during bulk registration works correctly
    /// </summary>
    [Fact]
    public void AsSelf_Works()
    {
        // arrange
        Container.Add(_ab.AsEnumerable()).AsSelf().Singleton();

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
        Container.Add(_ab.AsEnumerable()).As(typeof(A)).Singleton();

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
        Container.Add(_ab.AsEnumerable()).AsInterfaces().Singleton();

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
        Container.Add(_ab.AsEnumerable()).AsKeyedSelf(x => x.Name).Singleton();

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
        Container.Add(_ab.AsEnumerable()).AsKeyed(typeof(A), x => x.Name).Singleton();

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
        Container.Add(_ab.AsEnumerable()).AsSelfFactory().Singleton();

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
        Container.Add(_ab.AsEnumerable()).AsFactory<A>().Singleton();

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
        Container.Add(_ab.AsEnumerable()).AsKeyedSelfFactory(x => x.Name).Singleton();

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
        Container.Add(_ab.AsEnumerable()).AsKeyedFactory(typeof(A), x => x.Name).Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        GetKeyed<Func<A>>(nameof(A))().AsExact<A>();
        GetKeyed<Func<A>>(nameof(B))().AsExact<B>();
    }

    /// <summary>
    /// Verifies that registering types as keyed interfaces during bulk registration works correctly
    /// </summary>
    [Fact]
    public void AsKeyedInterfaces_Works()
    {
        // arrange
        Container.Add(_ab.AsEnumerable()).AsKeyedInterfaces(x => x.Name).Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        GetKeyed<IA>(nameof(A)).AsExact<A>();
        GetKeyed<IA>(nameof(B)).AsExact<B>();
        GetKeyed<IB>(nameof(B)).AsExact<B>();
    }

    /// <summary>
    /// Verifies that bulk registration with scoped lifetime produces scoped descriptors
    /// </summary>
    [Fact]
    public void Scoped_Works()
    {
        // arrange
        Container.Add(_aOnly.AsEnumerable()).AsSelf().Scoped();

        // act
        Build();

        // assert
        Container.HasScoped(typeof(A), typeof(A));
    }

    /// <summary>
    /// Verifies that bulk registration with transient lifetime produces transient descriptors
    /// </summary>
    [Fact]
    public void Transient_Works()
    {
        // arrange
        Container.Add(_aOnly.AsEnumerable()).AsSelf().Transient();

        // act
        Build();

        // assert
        Container.HasTransient(typeof(A), typeof(A));
    }

    /// <summary>
    /// Verifies that the In(lifetime) overload accepts a ServiceLifetime argument for bulk registration
    /// </summary>
    [Fact]
    public void In_AcceptsLifetime()
    {
        // arrange
        Container.Add(_aOnly.AsEnumerable()).AsSelf().In(ServiceLifetime.Scoped);

        // act
        Build();

        // assert
        Container.HasScoped(typeof(A), typeof(A));
    }

    /// <summary>
    /// Verifies that explicit AsSelf followed by As does not produce a duplicate self-registration
    /// </summary>
    [Fact]
    public void AsSelfThenAs_DoesNotDoubleRegisterSelf()
    {
        // arrange
        Container.Add(_aOnly.AsEnumerable()).AsSelf().As(typeof(IA)).Singleton();

        // act
        Build();

        // assert
        // exactly one descriptor for A as itself — the explicit AsSelf; no implicit second self-registration
        Container.HasSingleton(typeof(A), typeof(A));
        Container.Has(typeof(A), 1);
    }

    /// <summary>
    /// Verifies that a Where predicate that matches nothing registers no descriptors
    /// </summary>
    [Fact]
    public void Where_EmptyResult_RegistersNothing()
    {
        // arrange
        Container.Add(_ab.AsEnumerable()).Where(_ => false).AsSelf().Singleton();

        // act
        Build();

        // assert
        Container.Has(typeof(A), 0);
        Container.Has(typeof(B), 0);
    }

    /// <summary>
    /// Verifies that calling a lifetime terminator without specifying registration targets throws
    /// </summary>
    [Fact]
    public void In_NoRegistrationTargets_Throws()
    {
        Wrap.It(() => Container.Add(_ab.AsEnumerable()).Singleton())
            .Throws<InvalidOperationException>()
            .Reports("Specify registration targets");
    }

    /// <summary>
    /// Verifies that calling a lifetime terminator a second time on the same bulk builder throws
    /// </summary>
    [Fact]
    public void In_RegistrarReused_Throws()
    {
        var b = Container.Add(_aOnly.AsEnumerable()).AsSelf();
        b.Singleton();
        Wrap.It(() => b.Singleton()).Throws<InvalidOperationException>().Reports(Registrar.AlreadyRegisteredMessage);
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
