using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

public class KeyedFactoryRegistrationTest : TestBase
{
    [Fact]
    public void AsKeyedSelf_Works()
    {
        // arrange
        D? instance = null;
        Container.Add((_, key) => new D(key.ToString().NotNull())).AsKeyedSelf("x").Singleton();

        // act
        Build();

        // assert
        GetKeyed<D>("x")
            .Is(instance);
        instance.IsNotDefault();
        instance.Key.Is("x");
    }

    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        D? instance = null;
        Container.Add((_, key) => new D(key.ToString().NotNull())).AsKeyed<C>("x").Singleton();

        // act
        Build();

        // assert
        GetKeyed<C>("x")
            .Is(instance);
        instance.IsNotDefault();
        instance.Key.Is("x");
    }

    [Fact]
    public void AsKeyedInterfaces_Works()
    {
        // arrange
        D? instance = null;
        Container.Add((_, key) => new D(key.ToString().NotNull())).AsKeyedInterfaces("x").Singleton();

        // act
        Build();

        // assert
        Container.HasSingletonTypeFactory(typeof(IX), "x");
        GetKeyed<IX>("x").Is(instance);
        instance.IsNotDefault();
        instance.Key.Is("x");
    }

    private sealed class D : C, IX
    {
        private static int _count;
        public string Key { get; }

        public D(string key)
        {
            Key = key;
            if (_count++ > 1)
                throw new Exception("singleton failed");
        }
    }

    private class C;

    private interface IX;
}
