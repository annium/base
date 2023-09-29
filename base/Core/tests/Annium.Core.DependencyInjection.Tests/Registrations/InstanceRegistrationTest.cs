using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

public class InstanceRegistrationTest : TestBase
{
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

    [Fact]
    public void AsKeyedSelf_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsKeyedSelf(nameof(D)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, D>>()[nameof(D)].Is(instance);
    }

    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsKeyed<C, string>(nameof(D)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, C>>()[nameof(D)].Is(instance);
    }

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

    [Fact]
    public void AsKeyedSelfFactory_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsKeyedSelfFactory(nameof(D)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, Func<D>>>()[nameof(D)]().Is(instance);
    }

    [Fact]
    public void AsKeyedFactory_Works()
    {
        // arrange
        var instance = new D(new A());
        Container.Add(instance).AsKeyedFactory<C, string>(nameof(C)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, Func<C>>>()[nameof(C)]().Is(instance);
    }

    private sealed class D : C, ID
    {
        public D(A x) : base(x)
        {
        }
    }

    private class C : IC
    {
        // ReSharper disable once UnusedParameter.Local
        protected C(A _)
        {
        }
    }

    private interface ID : IC
    {
    }

    private interface IC
    {
    }

    private class A
    {
    }
}