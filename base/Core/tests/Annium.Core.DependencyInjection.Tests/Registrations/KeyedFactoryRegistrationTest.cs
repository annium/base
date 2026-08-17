using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

/// <summary>
/// Tests for keyed factory registration functionality in the dependency injection container
/// </summary>
public class KeyedFactoryRegistrationTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyedFactoryRegistrationTest"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public KeyedFactoryRegistrationTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that keyed factory registration as self works correctly
    /// </summary>
    [Fact]
    public void AsKeyedSelf_Works()
    {
        // arrange
        D.Reset();
        D? instance = null;
        Container.Add((_, key) => instance = new D(key.ToString().NotNull())).AsKeyedSelf("x").Singleton();

        // act
        Build();

        // assert
        GetKeyed<D>("x").Is(instance);
        instance.IsNotDefault();
        instance.Key.Is("x");
    }

    /// <summary>
    /// Verifies that keyed factory registration as specific type works correctly
    /// </summary>
    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        D.Reset();
        D? instance = null;
        Container.Add((_, key) => instance = new D(key.ToString().NotNull())).AsKeyed<C>("x").Singleton();

        // act
        Build();

        // assert
        GetKeyed<C>("x").Is(instance);
        instance.IsNotDefault();
        instance.Key.Is("x");
    }

    /// <summary>
    /// Verifies that keyed factory registration as interfaces works correctly
    /// </summary>
    [Fact]
    public void AsKeyedInterfaces_Works()
    {
        // arrange
        D.Reset();
        D? instance = null;
        Container.Add((_, key) => instance = new D(key.ToString().NotNull())).AsKeyedInterfaces("x").Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonTypeFactory(typeof(IX), "x");
        GetKeyed<IX>("x").Is(instance);
        instance.IsNotDefault();
        instance.Key.Is("x");
    }

    /// <summary>
    /// Verifies that keyed factory registration as keyed self factory works correctly
    /// </summary>
    [Fact]
    public void AsKeyedSelfFactory_Works()
    {
        // arrange
        D.Reset();
        Container.Add((_, key) => new D(key.ToString().NotNull())).AsKeyedSelfFactory("x").Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonFuncFactory(typeof(D), "x");
        var result = GetKeyed<Func<D>>("x")();
        result.AsExact<D>();
        result.Key.Is("x");
    }

    /// <summary>
    /// Verifies that keyed factory registration as keyed factory of a base type works correctly
    /// </summary>
    [Fact]
    public void AsKeyedFactory_Works()
    {
        // arrange
        D.Reset();
        Container.Add((_, key) => new D(key.ToString().NotNull())).AsKeyedFactory<C>("x").Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonFuncFactory(typeof(C), "x");
        GetKeyed<Func<C>>("x")().AsExact<D>();
    }

    /// <summary>
    /// Verifies that calling Singleton() without specifying registration targets throws
    /// </summary>
    [Fact]
    public void In_NoRegistrationTargets_Throws()
    {
        // act & assert
        Wrap.It(() => Container.Add(typeof(D), (_, _) => new D("x")).Singleton())
            .Throws<InvalidOperationException>()
            .Reports("Specify registration targets");
    }

    /// <summary>
    /// Verifies that keyed factory registration with scoped lifetime produces scoped descriptors
    /// </summary>
    [Fact]
    public void Scoped_Works()
    {
        // arrange
        D.Reset();
        Container.Add((_, key) => new D(key.ToString().NotNull())).AsKeyedSelf("x").Scoped();

        // act
        Build();

        // assert
        Container.HasScopedTypeFactory(typeof(D), "x");
    }

    /// <summary>
    /// Verifies that keyed factory registration with transient lifetime produces transient descriptors
    /// </summary>
    [Fact]
    public void Transient_Works()
    {
        // arrange
        D.Reset();
        Container.Add((_, key) => new D(key.ToString().NotNull())).AsKeyedSelf("x").Transient();

        // act
        Build();

        // assert
        Container.HasTransientTypeFactory(typeof(D), "x");
    }

    /// <summary>
    /// Verifies that the In(lifetime) overload accepts a ServiceLifetime argument for keyed factory registration
    /// </summary>
    [Fact]
    public void In_AcceptsLifetime()
    {
        // arrange
        D.Reset();
        Container.Add((_, key) => new D(key.ToString().NotNull())).AsKeyedSelf("x").In(ServiceLifetime.Scoped);

        // act
        Build();

        // assert
        Container.HasScopedTypeFactory(typeof(D), "x");
    }

    /// <summary>
    /// Test class D that inherits from C and implements IX
    /// </summary>
    private sealed class D : C, IX
    {
        /// <summary>
        /// Counter for tracking instance creation
        /// </summary>
        private static int _count;

        /// <summary>
        /// Resets the instance count for testing purposes
        /// </summary>
        public static void Reset() => _count = 0;

        /// <summary>
        /// Gets the key associated with this instance
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="D"/> class.
        /// </summary>
        /// <param name="key">Lookup key.</param>
        public D(string key)
        {
            Key = key;
            if (++_count > 1)
                throw new Exception("singleton failed");
        }
    }

    /// <summary>
    /// Test class C
    /// </summary>
    private class C;

    /// <summary>
    /// Test interface IX
    /// </summary>
    private interface IX;
}
