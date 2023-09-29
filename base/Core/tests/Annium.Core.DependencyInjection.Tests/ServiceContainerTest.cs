using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests;

public class ServiceContainerTest : TestBase
{
    [Fact]
    public void Add_WritesToCollectionImmediately()
    {
        // arrange
        var instance = new A();

        // act
        Container.Add(instance).AsSelf().Singleton();
        Build();

        // assert
        Get<A>().Is(instance);
    }

    [Fact]
    public void ContainsType_Works()
    {
        // arrange
        Container.Add<A>().AsSelf().Singleton();

        // assert
        Container.Contains(ServiceDescriptor.Type(typeof(A), typeof(A), ServiceLifetime.Singleton)).IsTrue();
    }

    [Fact]
    public void ContainsFactory_Works()
    {
        // arrange
        static B Factory(IServiceProvider _) => new();
        Container.Add(Factory).AsSelf().Singleton();

        // assert
        Container.Contains(ServiceDescriptor.Factory(typeof(B), Factory, ServiceLifetime.Singleton)).IsTrue();
    }

    [Fact]
    public void ContainsInstance_Works()
    {
        // arrange
        var instance = new B();
        Container.Add(instance).AsSelf().Singleton();

        // assert
        Container.Contains(ServiceDescriptor.Instance(typeof(B), instance, ServiceLifetime.Singleton)).IsTrue();
    }

    private sealed record B : A;

    private record A;
}