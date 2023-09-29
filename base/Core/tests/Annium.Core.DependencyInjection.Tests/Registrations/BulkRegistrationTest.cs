using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

public class BulkRegistrationTest : TestBase
{
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
        Get<IIndex<string, A>>()[nameof(A)].Is(Get<A>());
        Get<IIndex<string, B>>()[nameof(B)].Is(Get<B>());
    }

    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).AsKeyed(typeof(A), x => x.Name).Singleton();

        // act
        Build();

        // assert
        var index = Get<IIndex<string, A>>();
        index[nameof(A)].AsExact<A>();
        index[nameof(B)].AsExact<B>();
    }

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
        Get<IIndex<string, Func<A>>>()[nameof(A)]().AsExact<A>();
        Get<IIndex<string, Func<B>>>()[nameof(B)]().AsExact<B>();
    }

    [Fact]
    public void AsKeyedFactory_Works()
    {
        // arrange
        Container.Add(new[] { typeof(A), typeof(B) }.AsEnumerable()).AsKeyedFactory(typeof(A), x => x.Name).Singleton();

        // act
        Build();

        // assert
        Container.HasSingleton(typeof(A), typeof(A));
        Container.HasSingleton(typeof(B), typeof(B));
        var index = Get<IIndex<string, Func<A>>>();
        index[nameof(A)]().AsExact<A>();
        index[nameof(B)]().AsExact<B>();
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