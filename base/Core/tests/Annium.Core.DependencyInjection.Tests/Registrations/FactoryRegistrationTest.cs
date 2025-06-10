using Annium.Core.DependencyInjection.Builders;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

/// <summary>
/// Tests for factory registration functionality in the dependency injection container
/// </summary>
public class FactoryRegistrationTest : TestBase
{
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
    /// Test interface IX
    /// </summary>
    private interface IX;
}
