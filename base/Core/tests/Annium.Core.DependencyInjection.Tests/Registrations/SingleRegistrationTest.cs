using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

public class SingleRegistrationTest : TestBase
{
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

    [Fact]
    public void AsKeyedSelf_Works()
    {
        // arrange
        B.Reset();
        Container.Add(typeof(B)).AsKeyedSelf(nameof(B)).Singleton();

        // act
        Build();

        // assert
        var index = Get<IIndex<string, B>>();
        B.InstancesCount.Is(0);
        index[nameof(B)].Is(Get<B>());
        B.InstancesCount.Is(1);
    }

    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        B.Reset();
        Container.Add(typeof(B)).AsKeyed(typeof(A), nameof(B)).Singleton();

        // act
        Build();

        // assert
        var index = Get<IIndex<string, A>>();
        B.InstancesCount.Is(0);
        index[nameof(B)].Is(Get<B>());
        B.InstancesCount.Is(1);
    }

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

    [Fact]
    public void AsKeyedSelfFactory_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsKeyedSelfFactory(nameof(B)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, Func<B>>>()[nameof(B)]().Is(Get<B>());
    }

    [Fact]
    public void AsKeyedFactory_Works()
    {
        // arrange
        Container.Add(typeof(B)).AsKeyedFactory(typeof(A), nameof(B)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, Func<A>>>()[nameof(B)]().Is(Get<B>());
    }

    private sealed class B : A, IB
    {
        public static void Reset()
        {
            InstancesCount = 0;
        }

        public static int InstancesCount { get; private set; }

        public B()
        {
            InstancesCount++;
        }
    }

    private class A : IA
    {
    }

    private interface IB : IA
    {
    }

    private interface IA
    {
    }
}