using System;
using Annium.Core.DependencyInjection.Builders;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

/// <summary>
/// Tests for keyed factory registration functionality in the dependency injection container
/// </summary>
public class KeyedFactoryRegistrationTest : TestBase
{
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
