using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

/// <summary>
/// Tests for factory registration functionality in the dependency injection container
/// </summary>
public class FactoryRegistrationTest : TestBase
{
    public FactoryRegistrationTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that factory registration as self works correctly
    /// </summary>
    [Fact]
    public void AsSelf_Works()
    {
        // arrange
        var a = new A();
        Container.Add(_ => new D(a)).AsSelf().Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonTypeFactory(typeof(D));
        var d = Get<D>();
        d.A.Is(a);
        Get<D>().Is(d);
    }

    /// <summary>
    /// Verifies that factory registration as specific type works correctly
    /// </summary>
    [Fact]
    public void As_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(_ => instance).As<C>().Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonTypeFactory(typeof(C));
        Get<C>().Is(instance);
    }

    /// <summary>
    /// Verifies that factory registration as interfaces works correctly
    /// </summary>
    [Fact]
    public void AsInterfaces_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(_ => instance).AsInterfaces().Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonTypeFactory(typeof(IX));
        Get<IX>().Is(instance);
    }

    /// <summary>
    /// Verifies that factory registration as self factory works correctly
    /// </summary>
    [Fact]
    public void AsSelfFactory_Works()
    {
        // arrange
        var a = new A();
        Container.Add(_ => new D(a)).AsSelfFactory().Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonFuncFactory(typeof(D));
        Get<Func<D>>()().AsExact<D>();
    }

    /// <summary>
    /// Verifies that factory registration as factory of a base type works correctly
    /// </summary>
    [Fact]
    public void AsFactory_Works()
    {
        // arrange
        var a = new A();
        Container.Add(_ => new D(a)).AsFactory<C>().Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonFuncFactory(typeof(C));
        Get<Func<C>>()().AsExact<D>();
    }

    /// <summary>
    /// Verifies that factory registration as keyed self works correctly
    /// </summary>
    [Fact]
    public void AsKeyedSelf_Works()
    {
        // arrange
        var a = new A();
        Container.Add(_ => new D(a)).AsKeyedSelf(nameof(D)).Singleton();

        // act
        Build();

        // assert
        GetKeyed<D>(nameof(D)).AsExact<D>();
    }

    /// <summary>
    /// Verifies that factory registration as keyed base type works correctly
    /// </summary>
    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        var a = new A();
        Container.Add(_ => new D(a)).AsKeyed<C>(nameof(D)).Singleton();

        // act
        Build();

        // assert
        GetKeyed<C>(nameof(D)).AsExact<D>();
    }

    /// <summary>
    /// Verifies that factory registration as keyed interfaces works correctly
    /// </summary>
    [Fact]
    public void AsKeyedInterfaces_Works()
    {
        // arrange
        var instance = new E(new A());
        Container.Add(_ => instance).AsKeyedInterfaces(nameof(E)).Singleton();

        // act
        Build();

        // assert
        Container.Has(typeof(IX), 1);
        Container.Has(typeof(IY), 1);
        GetKeyed<IX>(nameof(E)).Is(instance);
        GetKeyed<IY>(nameof(E)).Is(instance);
    }

    /// <summary>
    /// Verifies that factory registration as keyed self factory works correctly
    /// </summary>
    [Fact]
    public void AsKeyedSelfFactory_Works()
    {
        // arrange
        var a = new A();
        Container.Add(_ => new D(a)).AsKeyedSelfFactory(nameof(D)).Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonFuncFactory(typeof(D), nameof(D));
        GetKeyed<Func<D>>(nameof(D))().AsExact<D>();
    }

    /// <summary>
    /// Verifies that factory registration as keyed factory of a base type works correctly
    /// </summary>
    [Fact]
    public void AsKeyedFactory_Works()
    {
        // arrange
        var a = new A();
        Container.Add(_ => new D(a)).AsKeyedFactory<C>(nameof(D)).Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonFuncFactory(typeof(C), nameof(D));
        GetKeyed<Func<C>>(nameof(D))().AsExact<D>();
    }

    /// <summary>
    /// Verifies that calling Singleton() without specifying registration targets throws
    /// </summary>
    [Fact]
    public void In_NoRegistrationTargets_Throws()
    {
        // act & assert
        Wrap.It(() => Container.Add(typeof(D), _ => new D(new A())).Singleton())
            .Throws<InvalidOperationException>()
            .Reports("Specify registration targets");
    }

    /// <summary>
    /// Verifies that factory registration with scoped lifetime produces scoped descriptors
    /// </summary>
    [Fact]
    public void Scoped_Works()
    {
        // arrange
        var a = new A();
        Container.Add(_ => new D(a)).AsSelf().Scoped();

        // act
        Build();

        // assert
        Container.HasScopedTypeFactory(typeof(D));
    }

    /// <summary>
    /// Verifies that factory registration with transient lifetime produces transient descriptors
    /// </summary>
    [Fact]
    public void Transient_Works()
    {
        // arrange
        var a = new A();
        Container.Add(_ => new D(a)).AsSelf().Transient();

        // act
        Build();

        // assert
        Container.HasTransientTypeFactory(typeof(D));
    }

    /// <summary>
    /// Verifies that the In(lifetime) overload accepts a ServiceLifetime argument for factory registration
    /// </summary>
    [Fact]
    public void In_AcceptsLifetime()
    {
        // arrange
        var a = new A();
        Container.Add(_ => new D(a)).AsSelf().In(ServiceLifetime.Scoped);

        // act
        Build();

        // assert
        Container.HasScopedTypeFactory(typeof(D));
    }

    /// <summary>
    /// Test class E that inherits from C and implements IX and IY
    /// </summary>
    private sealed class E : C, IX, IY
    {
        public E(A x)
            : base(x) { }
    }

    /// <summary>
    /// Test class D that inherits from C and implements IX
    /// </summary>
    private sealed class D : C, IX
    {
        public D(A x)
            : base(x) { }
    }

    /// <summary>
    /// Test class C
    /// </summary>
    private class C
    {
        /// <summary>
        /// Gets the A instance
        /// </summary>
        public A A { get; }

        protected C(A a)
        {
            A = a;
        }
    }

    /// <summary>
    /// Test class A
    /// </summary>
    private class A;

    /// <summary>
    /// Test interface IY that extends IX
    /// </summary>
    private interface IY : IX;

    /// <summary>
    /// Test interface IX
    /// </summary>
    private interface IX;
}
